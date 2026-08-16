from __future__ import annotations

import argparse
import hashlib
import re
from pathlib import Path
from xml.sax.saxutils import escape

import arabic_reshaper
import pymupdf
from bidi.algorithm import get_display
from PIL import Image as PillowImage
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    Image,
    KeepTogether,
    ListFlowable,
    ListItem,
    PageBreak,
    PageTemplate,
    Paragraph,
    Spacer,
    Table,
    TableStyle,
)


BLUE = colors.HexColor("#0877C9")
INK = colors.HexColor("#142033")
MUTED = colors.HexColor("#5E6B7E")
PALE = colors.HexColor("#EEF5FA")
LINE = colors.HexColor("#D6E0E8")
WARNING = colors.HexColor("#FFF3D6")


def has_arabic(value: str) -> bool:
    return bool(re.search(r"[\u0600-\u06ff]", value))


def shape(value: str, rtl: bool) -> str:
    if not rtl:
        return value
    if has_arabic(value):
        value = arabic_reshaper.reshape(value)
    return get_display(value)


def clean_inline(value: str) -> str:
    value = re.sub(r"!\[([^]]*)\]\([^)]+\)", r"\1", value)
    value = re.sub(r"\[([^]]+)\]\([^)]+\)", r"\1", value)
    value = re.sub(r"[`*_]", "", value)
    return value.strip()


class GuideDocument(BaseDocTemplate):
    def __init__(self, filename: str, *, rtl: bool, guide_title: str, **kwargs):
        super().__init__(filename, **kwargs)
        self.rtl = rtl
        self.guide_title = guide_title
        frame = Frame(self.leftMargin, self.bottomMargin, self.width, self.height, id="normal")
        self.addPageTemplates(PageTemplate(id="guide", frames=frame, onPage=self.draw_page))

    def draw_page(self, canvas, doc):
        if doc.page == 1:
            return
        canvas.saveState()
        canvas.setStrokeColor(LINE)
        canvas.line(self.leftMargin, A4[1] - 15 * mm, A4[0] - self.rightMargin, A4[1] - 15 * mm)
        canvas.setFont("Arial", 8)
        canvas.setFillColor(MUTED)
        canvas.drawRightString(A4[0] - self.rightMargin, A4[1] - 11.5 * mm, shape(self.guide_title, self.rtl))
        canvas.drawCentredString(A4[0] / 2, 10 * mm, str(doc.page))
        canvas.restoreState()

    def afterFlowable(self, flowable):
        if isinstance(flowable, Paragraph) and hasattr(flowable, "toc_level"):
            text = getattr(flowable, "toc_text")
            key = f"section-{self.seq.nextf('section')}"
            self.canv.bookmarkPage(key)
            self.canv.addOutlineEntry(text, key, level=flowable.toc_level, closed=False)
            self.notify("TOCEntry", (flowable.toc_level, text, self.page, key))


def register_fonts(culture: str) -> None:
    normalized = culture.lower()
    if normalized.startswith(("zh-hans",)):
        regular = Path(r"C:\Windows\Fonts\msyh.ttc")
        bold = Path(r"C:\Windows\Fonts\msyhbd.ttc")
    elif normalized.startswith(("zh-hant",)):
        regular = Path(r"C:\Windows\Fonts\msjh.ttc")
        bold = Path(r"C:\Windows\Fonts\msjhbd.ttc")
    elif normalized.startswith("th"):
        regular = Path(r"C:\Windows\Fonts\LEELAWAD.TTF")
        bold = Path(r"C:\Windows\Fonts\LEELAWDB.TTF")
    elif normalized.startswith(("ja", "ko")):
        regular = Path(r"C:\Windows\Fonts\malgun.ttf")
        bold = Path(r"C:\Windows\Fonts\malgunbd.ttf")
    else:
        regular = Path(r"C:\Windows\Fonts\arial.ttf")
        bold = Path(r"C:\Windows\Fonts\arialbd.ttf")
    if not regular.exists() or not bold.exists():
        raise FileNotFoundError("Arial fonts are required to render the Arabic guide.")
    pdfmetrics.registerFont(TTFont("Arial", str(regular)))
    pdfmetrics.registerFont(TTFont("Arial-Bold", str(bold)))


def compressed_image(source: Path, cache: Path) -> Path:
    digest = hashlib.sha1(str(source.resolve()).encode("utf-8")).hexdigest()[:12]
    destination = cache / f"{source.stem}-{digest}.png"
    if destination.exists() and destination.stat().st_mtime >= source.stat().st_mtime:
        return destination
    cache.mkdir(parents=True, exist_ok=True)
    with PillowImage.open(source) as image:
        image = image.convert("RGB")
        if image.width > 740:
            height = round(image.height * 740 / image.width)
            image = image.resize((740, height), PillowImage.Resampling.LANCZOS)
        image = image.quantize(colors=96, method=PillowImage.Quantize.MEDIANCUT)
        image.save(destination, "PNG", optimize=True, compress_level=9)
    return destination


def parse_table(lines: list[str], start: int) -> tuple[list[list[str]], int]:
    rows: list[list[str]] = []
    index = start
    while index < len(lines) and lines[index].strip().startswith("|"):
        raw = [cell.strip() for cell in lines[index].strip().strip("|").split("|")]
        if not all(re.fullmatch(r":?-{3,}:?", cell) for cell in raw):
            rows.append(raw)
        index += 1
    return rows, index


def make_pdf(source: Path, destination: Path, *, culture: str) -> None:
    register_fonts(culture)
    rtl = culture.lower().startswith(("ar", "he", "fa", "ur"))
    alignment = TA_RIGHT if rtl else TA_LEFT
    lines = source.read_text(encoding="utf-8").splitlines()
    title = clean_inline(next(line[2:] for line in lines if line.startswith("# ")))
    localized_cover = {
        "cs-cz": (
            "Praktická příručka pro čtení, zápis, převod a kontrolu obrazů disků, nastavení emulace a používání nástrojů Greaseweazle.",
            "Vydání dokumentace: 16. srpna 2026",
        ),
        "da-dk": (
            "Praktisk vejledning til læsning, skrivning, konvertering og kontrol af diskaftryk samt konfiguration af emulering og brug af Greaseweazle-værktøjerne.",
            "Dokumentationsudgave: 16. august 2026",
        ),
        "de-de": (
            "Praktischer Leitfaden zum Lesen, Schreiben, Konvertieren und Prüfen von Diskettenabbildern sowie zum Konfigurieren der Emulation und Verwenden der Greaseweazle-Werkzeuge.",
            "Dokumentationsstand: 16. August 2026",
        ),
        "el-gr": (
            "Πρακτικός οδηγός για την ανάγνωση, εγγραφή, μετατροπή και επιθεώρηση εικόνων δίσκου, τη ρύθμιση της εξομοίωσης και τη χρήση των εργαλείων Greaseweazle.",
            "Έκδοση τεκμηρίωσης: 16 Αυγούστου 2026",
        ),
        "es-es": (
            "Guía práctica para leer, escribir, convertir e inspeccionar imágenes de disco, configurar la emulación y utilizar las herramientas de Greaseweazle.",
            "Edición de la documentación: 16 de agosto de 2026",
        ),
        "fi-fi": ("Käytännön opas levykuvien lukemiseen, kirjoittamiseen, muuntamiseen ja tarkastamiseen sekä emuloinnin ja Greaseweazle-työkalujen käyttöön.", "Dokumentaation julkaisu: 16. elokuuta 2026"),
        "he-il": ("מדריך מעשי לקריאה, כתיבה, המרה ובדיקה של דימויי דיסק, להגדרת אמולציה ולשימוש בכלי Greaseweazle.", "מהדורת התיעוד: 16 באוגוסט 2026"),
        "hu-hu": ("Gyakorlati útmutató lemezképek olvasásához, írásához, átalakításához és vizsgálatához, valamint az emuláció és a Greaseweazle eszközök használatához.", "Dokumentációs kiadás: 2026. augusztus 16."),
        "id-id": ("Panduan praktis untuk membaca, menulis, mengonversi dan memeriksa citra disk, mengatur emulasi, serta menggunakan alat Greaseweazle.", "Edisi dokumentasi: 16 Agustus 2026"),
        "it-it": ("Guida pratica alla lettura, scrittura, conversione e ispezione delle immagini disco, alla configurazione dell'emulazione e all'uso degli strumenti Greaseweazle.", "Edizione della documentazione: 16 agosto 2026"),
        "ja-jp": ("ディスクイメージの読み取り、書き込み、変換、検査、エミュレーションの設定、および Greaseweazle ツールの使用に関する実用ガイドです。", "ドキュメント版：2026年8月16日"),
        "ko-kr": ("디스크 이미지 읽기, 쓰기, 변환 및 검사와 에뮬레이션 설정, Greaseweazle 도구 사용을 위한 실용 안내서입니다.", "문서 버전: 2026년 8월 16일"),
        "nb-no": ("Praktisk veiledning for lesing, skriving, konvertering og kontroll av diskbilder samt oppsett av emulering og bruk av Greaseweazle-verktøyene.", "Dokumentasjonsutgave: 16. august 2026"),
        "nl-nl": ("Praktische handleiding voor het lezen, schrijven, converteren en inspecteren van schijfimages, het instellen van emulatie en het gebruiken van de Greaseweazle-hulpmiddelen.", "Documentatie-uitgave: 16 augustus 2026"),
        "pl-pl": ("Praktyczny przewodnik po odczytywaniu, zapisywaniu, konwertowaniu i sprawdzaniu obrazów dysków, konfigurowaniu emulacji oraz używaniu narzędzi Greaseweazle.", "Wydanie dokumentacji: 16 sierpnia 2026"),
        "pt-br": ("Guia prático para ler, gravar, converter e inspecionar imagens de disco, configurar a emulação e usar as ferramentas Greaseweazle.", "Edição da documentação: 16 de agosto de 2026"),
        "pt-pt": ("Guia prático para ler, escrever, converter e inspecionar imagens de disco, configurar a emulação e utilizar as ferramentas Greaseweazle.", "Edição da documentação: 16 de agosto de 2026"),
        "ro-ro": ("Ghid practic pentru citirea, scrierea, conversia și inspectarea imaginilor de disc, configurarea emulării și utilizarea instrumentelor Greaseweazle.", "Ediția documentației: 16 august 2026"),
        "ru-ru": ("Практическое руководство по чтению, записи, преобразованию и исследованию образов дисков, настройке эмуляции и использованию инструментов Greaseweazle.", "Выпуск документации: 16 августа 2026 г."),
        "sv-se": ("Praktisk guide för att läsa, skriva, konvertera och granska diskavbilder, konfigurera emulering och använda Greaseweazle-verktygen.", "Dokumentationsutgåva: 16 augusti 2026"),
        "th-th": ("คู่มือปฏิบัติสำหรับการอ่าน เขียน แปลงและตรวจสอบอิเมจดิสก์ การตั้งค่าการจำลอง และการใช้เครื่องมือ Greaseweazle", "ฉบับเอกสาร: 16 สิงหาคม 2026"),
        "tr-tr": ("Disk görüntülerini okuma, yazma, dönüştürme ve inceleme, emülasyonu yapılandırma ve Greaseweazle araçlarını kullanma konusunda uygulamalı kılavuz.", "Belge sürümü: 16 Ağustos 2026"),
        "uk-ua": ("Практичний посібник із читання, запису, перетворення та перевірки образів дисків, налаштування емуляції та використання інструментів Greaseweazle.", "Видання документації: 16 серпня 2026 р."),
        "vi-vn": ("Hướng dẫn thực hành về đọc, ghi, chuyển đổi và kiểm tra ảnh đĩa, cấu hình giả lập và sử dụng các công cụ Greaseweazle.", "Phiên bản tài liệu: 16 tháng 8 năm 2026"),
        "zh-hans": ("用于读取、写入、转换和检查磁盘映像、配置仿真以及使用 Greaseweazle 工具的实用指南。", "文档版本：2026 年 8 月 16 日"),
        "zh-hant": ("用於讀取、寫入、轉換和檢查磁碟映像、設定模擬以及使用 Greaseweazle 工具的實用指南。", "文件版本：2026 年 8 月 16 日"),
        "ar-sa": ("دليل عملي لقراءة صور الأقراص وكتابتها وتحويلها وفحصها، ولإعداد المحاكاة واستخدام أدوات Greaseweazle.", "إصدار الوثيقة: 16 أغسطس 2026"),
    }
    cover_lead, cover_date = localized_cover.get(
        culture.lower(),
        ("Practical guide", "Documentation edition: 16 August 2026"),
    )
    styles = getSampleStyleSheet()
    body = ParagraphStyle("Body", parent=styles["BodyText"], fontName="Arial", fontSize=9.4,
                          leading=13.5, textColor=INK, alignment=alignment, spaceAfter=3 * mm,
                          wordWrap="RTL" if rtl else None)
    h1 = ParagraphStyle("H1", parent=body, fontName="Arial-Bold", fontSize=25, leading=33,
                        textColor=INK, alignment=TA_CENTER, spaceAfter=7 * mm)
    h2 = ParagraphStyle("H2", parent=body, fontName="Arial-Bold", fontSize=17, leading=23,
                        textColor=BLUE, spaceBefore=6 * mm, spaceAfter=3 * mm, keepWithNext=True)
    h3 = ParagraphStyle("H3", parent=body, fontName="Arial-Bold", fontSize=12.5, leading=17,
                        textColor=INK, spaceBefore=4 * mm, spaceAfter=2 * mm, keepWithNext=True)
    caption = ParagraphStyle("Caption", parent=body, fontSize=8, leading=10, textColor=MUTED,
                             alignment=TA_CENTER, spaceBefore=1.5 * mm, spaceAfter=4 * mm)
    quote = ParagraphStyle("Quote", parent=body, backColor=WARNING, borderColor=colors.HexColor("#E9B949"),
                           borderWidth=0.8, borderPadding=7, spaceBefore=2 * mm, spaceAfter=3 * mm)
    cell = ParagraphStyle("Cell", parent=body, fontSize=7.4, leading=9.5, spaceAfter=0)
    cell_head = ParagraphStyle("CellHead", parent=cell, fontName="Arial-Bold", textColor=colors.white)
    toc_style = ParagraphStyle("TOC", parent=body, fontSize=9.5, leading=13, spaceAfter=0)

    toc_entries: list[str] = []
    in_contents = False
    first_h2 = True
    for source_line in lines:
        if source_line.startswith("## "):
            heading_name = clean_inline(source_line[3:])
            if first_h2:
                in_contents = True
                first_h2 = False
                continue
            if in_contents:
                break
        if in_contents:
            match = re.match(r"^\d+\.\s+(.+)$", source_line.strip())
            if match:
                toc_entries.append(clean_inline(match.group(1)))

    destination.parent.mkdir(parents=True, exist_ok=True)
    cache = Path("tmp/pdfs/assets")
    story = [Spacer(1, 42 * mm), Paragraph(shape("GW GUI", rtl), h1),
             Paragraph(shape(title.replace("GW GUI", "").strip(" -—"), rtl),
                       ParagraphStyle("Subtitle", parent=h1, fontSize=19, leading=25, textColor=BLUE)),
             Spacer(1, 12 * mm),
             Paragraph(shape(cover_lead, rtl),
                       ParagraphStyle("Lead", parent=body, fontSize=12, leading=19, alignment=TA_CENTER,
                                      textColor=MUTED)),
             Spacer(1, 55 * mm),
             Paragraph(shape(cover_date, rtl), caption),
             PageBreak()]

    paragraph_lines: list[str] = []
    list_items: list[str] = []
    skipping_source_toc = False
    first_story_h2 = True

    def flush_paragraph() -> None:
        if paragraph_lines:
            text = clean_inline(" ".join(part.strip() for part in paragraph_lines))
            story.append(Paragraph(escape(shape(text, rtl)), body))
            paragraph_lines.clear()

    def flush_list() -> None:
        if list_items:
            items = [ListItem(Paragraph(escape(shape(clean_inline(item), rtl)), body), leftIndent=3 * mm)
                     for item in list_items]
            story.append(ListFlowable(items, bulletType="bullet", start="circle", leftIndent=8 * mm,
                                      rightIndent=8 * mm, bulletFontName="Arial"))
            story.append(Spacer(1, 2 * mm))
            list_items.clear()

    index = 0
    while index < len(lines):
        line = lines[index]
        stripped = line.strip()
        image_match = re.search(r'<img src="([^"]+)" alt="([^"]+)"', stripped)
        if line.startswith("# "):
            index += 1
            continue
        if line.startswith("## "):
            flush_paragraph(); flush_list()
            heading = clean_inline(line[3:])
            if first_story_h2:
                first_story_h2 = False
                story.append(Paragraph(shape(heading, rtl), h2))
                toc_rows = []
                for entry in toc_entries:
                    entry_cell = Paragraph(escape(shape(entry, rtl)), toc_style)
                    toc_rows.append([entry_cell])
                toc_table = Table(toc_rows, colWidths=[165 * mm])
                toc_table.setStyle(TableStyle([
                    ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                    ("ALIGN", (0, 0), (-1, -1), "RIGHT" if rtl else "LEFT"),
                    ("BOTTOMPADDING", (0, 0), (-1, -1), 2.5),
                    ("TOPPADDING", (0, 0), (-1, -1), 2.5),
                    ("LINEBELOW", (0, 0), (-1, -2), 0.2, LINE),
                ]))
                story.append(toc_table)
                story.append(PageBreak())
                skipping_source_toc = True
            else:
                skipping_source_toc = False
                paragraph = Paragraph(escape(shape(heading, rtl)), h2)
                paragraph.toc_level = 0
                paragraph.toc_text = heading
                story.append(paragraph)
            index += 1
            continue
        if skipping_source_toc and (re.match(r"^\d+\.\s", stripped) or not stripped):
            index += 1
            continue
        if line.startswith("### ") or line.startswith("#### "):
            flush_paragraph(); flush_list()
            story.append(Paragraph(escape(shape(clean_inline(line.lstrip("# ")), rtl)), h3))
            index += 1
            continue
        if image_match:
            flush_paragraph(); flush_list()
            image_path = source.parent / image_match.group(1)
            prepared = compressed_image(image_path, cache)
            with PillowImage.open(prepared) as bitmap:
                width = min(155 * mm, bitmap.width * 0.19 * mm)
                height = width * bitmap.height / bitmap.width
            figure = Image(str(prepared), width=width, height=height)
            figure.hAlign = "CENTER"
            story.append(KeepTogether([figure, Paragraph(escape(shape(image_match.group(2), rtl)), caption)]))
            index += 1
            continue
        if stripped.startswith("|"):
            flush_paragraph(); flush_list()
            rows, index = parse_table(lines, index)
            if rows:
                rendered = [[Paragraph(escape(shape(clean_inline(value), rtl)), cell_head if row_index == 0 else cell)
                             for value in row] for row_index, row in enumerate(rows)]
                columns = len(rendered[0])
                table = Table(rendered, colWidths=[170 * mm / columns] * columns, repeatRows=1, hAlign="RIGHT" if rtl else "LEFT")
                table.setStyle(TableStyle([
                    ("BACKGROUND", (0, 0), (-1, 0), BLUE), ("GRID", (0, 0), (-1, -1), 0.35, LINE),
                    ("VALIGN", (0, 0), (-1, -1), "TOP"), ("RIGHTPADDING", (0, 0), (-1, -1), 5),
                    ("LEFTPADDING", (0, 0), (-1, -1), 5), ("TOPPADDING", (0, 0), (-1, -1), 4),
                    ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
                    ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, PALE]),
                ]))
                story.extend([table, Spacer(1, 3 * mm)])
            continue
        if stripped.startswith(">"):
            flush_paragraph(); flush_list()
            story.append(Paragraph(escape(shape(clean_inline(stripped.lstrip("> ")), rtl)), quote))
            index += 1
            continue
        item = re.match(r"^(?:[-*+] |\d+\. )(.*)$", stripped)
        if item:
            flush_paragraph()
            list_items.append(item.group(1))
            index += 1
            continue
        if not stripped or stripped == "---":
            flush_paragraph(); flush_list()
            index += 1
            continue
        paragraph_lines.append(stripped)
        index += 1
    flush_paragraph(); flush_list()

    document = GuideDocument(str(destination), rtl=rtl, guide_title=title, pagesize=A4,
                             leftMargin=18 * mm, rightMargin=18 * mm,
                             topMargin=21 * mm, bottomMargin=16 * mm,
                             title=title, author="GW GUI project", subject=f"GW GUI user guide ({culture})")
    document.multiBuild(story)
    with pymupdf.open(destination) as pdf:
        if pdf.page_count < 10:
            raise RuntimeError("Generated guide has unexpectedly few pages.")
    print(f"Written {destination} ({destination.stat().st_size} bytes)")


def main() -> None:
    parser = argparse.ArgumentParser(description="Build a compact localized GW GUI user-guide PDF.")
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument("--culture", required=True)
    args = parser.parse_args()
    make_pdf(args.source, args.destination, culture=args.culture)


if __name__ == "__main__":
    main()
