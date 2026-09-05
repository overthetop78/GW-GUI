[🌐 Languages / Langues](../Home.md)

# GW GUI Panduan Pengguna

GW GUI adalah aplikasi Windows untuk membaca, menulis, mengubah, memeriksa, dan meniru gambar disket floppy-. Ini dapat mengontrol Greaseweazle perangkat keras, bekerja dengan file disk-image melalui mesin internal, dan menjalankan konfigurasi emulated- mesin disimpan.

Panduan ini menggambarkan antar muka Inggris yang ditampilkan pada versi aplikasi saat ini. Ini ditulis sebagai sumber manual pengguna yang dapat dicetak: cuplikan layar mengilustrasikan kendali, sedangkan teks di sekitarnya menjelaskan apa yang harus dipilih, mengapa memilihnya, dan bagaimana memverifikasi hasilnya.

> **Penting:** Membaca disk tidak merusak. Menulis, menghapus, memperbarui firmware, dan beberapa perangkat keras dapat memodifikasi media atau perangkat keras. Baca peringatan yang dicantolkan ke prosedur yang relevan sebelum mengklik ** Jalankan**.

### Bagaimana menggunakan panduan ini

Jika ini adalah pertama kalinya Anda menggunakan GW GUI, lengkap [Mendapatkan dimulai](#getting-started), kemudian mengikuti [Membaca disk](#reading-a-disk)Jika aplikasi telah dikonfigurasi, pergi langsung ke bab untuk operasi yang ingin Anda lakukan. Bab-bab opsi berfungsi sebagai referensi ketika prosedur meminta Anda untuk mengubah drive, mesin, profil, atau emulated- pengaturan mesin.

Nama antarmuka ditampilkan pada **tebal** Nama berkas, jalur, perintah, dan nilai literal ditampilkan sebagai `code`. Catatan menjelaskan perilaku normal; peringatan mengidentifikasi operasi yang dapat mengubah disk, pengontrol, atau konfigurasi tersimpan.

## Isi

1. [Memahami alur kerja](#understanding-the-workflow)
2. [Memulai](#getting-started)
3. [Jendela utama](#main-window)
4. [Membaca disk](#reading-a-disk)
5. [Menulis disk](#writing-a-disk)
6. [Mengkonversi gambar disk](#converting-disk-images)
7. [Memvisualisasikan suatu image disk](#visualizing-a-disk-image)
8. [Menjelajahi isi disk](#exploring-disk-contents)
9. [Menggunakan alat](#using-the-tools)
10. [Emulasi](#emulation)
11. [Opsi aplikasi](#application-options)
12. [Opsi emulasi](#emulation-options)
13. [Amiga konfigurasi](#amiga-configuration)
14. [Diagnosa dan pemeliharaan Perangkat Keras](#hardware-diagnostics-and-maintenance)
15. [Log dan riwayat operasi](#logs-and-operation-history)
16. [Data aplikasi dan penggunaan portabel](#application-data-and-portable-use)
17. [Direkomendasikan mengalir kerja](#recommended-workflows)
18. [Daftar cek keselamatan](#safety-checklist)
19. [Teriakan Masalah](#troubleshooting)
20. [Glossary](#glossary)
21. [Referensi cepat](#quick-reference)

## Memahami alur kerja

GW GUI memisahkan operasi fixcal- disk dari operasi berkas image-:

| Gol | Masukan | Keluaran | Halaman yang direkomendasikan |
|---|---|---|---|
| Mempertahankan disk floppy | Disk fisik | Berkas gambar | **Baca** |
| Recreate a floppy disk | Berkas gambar | Disk fisik | **Tulis** |
| Ubah format gambar | Berkas gambar | Satu atau lebih berkas citra | **Konversi** |
| Inspeksi trek dan anomali | Berkas gambar | Analisis visual | **Visualisasi** |
| Ramban berkas yang tersimpan dalam gambar | Sistem gambar / berkas yang didukung | Berkas dan direktori | **Disk Explorer** |
| Diagnosa drive atau controller | Greaseweazle hardware | Pengukuran atau status | **Perkakas** |
| Jalankan mesin virtual yang disimpan | Konfigurasi mesin tersimpan | Sesi emulasi | **Emulasi** |

Untuk pelestarian, pertama membuat penangkapan mentah dan tetap tidak berubah sebagai master. Buat salinan kerja yang dikonversi atau diperbaiki dari master itu. Ini menghindari mengulangi fisik membaca dan mempertahankan informasi bahwa format sektor- berbasis tidak dapat mempertahankan.

## Memulai

### Permintaan

- Jendela dengan Microsoft .NET Desktop Runtime dibutuhkan oleh aplikasi.
- A Greaseweazle pengontrol untuk operasi physical floppy- disk.
- Path yang dikonfigurasi ke `gw.exe` ketika menggunakan Greaseweazle Host Tools Mesin.
- Diperoleh secara sah ROM berkas ketika sebuah mesin beremulasi membutuhkan mereka.

Aplikasi ini harus diperiksa. Waktu berjalan di awal. Jika hilang, ikuti prompt instalasi, kemudian jalankan ulang GW GUI.

### Sebelum menghubungkan perangkat keras

Periksa berikut sebelum menjalankan operasi physical- disk:

1. Hubungkan Greaseweazle pengontrol ke stabil USB Port.
2. Hubungkan kabel floppy dengan orientasi yang benar.
3. Hubungkan pasokan daya drive sebelum memasukkan media berharga.
4. Konfirmasikan bahwa ukuran drive dan kepadatan cocok dengan disk.
5. Write- lindungi disk sumber ketika memungkinkan.

GW GUI tidak dapat mencegah kerusakan yang disebabkan oleh kabel yang salah, kekuatan yang tidak cocok, atau drive mekanis tidak aman. Uji perangkat keras asing dengan disk yang dibuang terlebih dahulu.

### Peluncuran pertama

1. Buka `gwgui.exe`.
2. Buka **Opsi**.
3. Masuk **Kontrol dan drive**, memindai pengontrol dan mengatur drive.
4. Verifikasi atau pilih lokasi ke `gw.exe`.
5. Masuk **Mesin**, pilih mesin mana yang harus melakukan setiap operasi.
6. Kembali ke jendela utama dan pilih tab operasi yang diperlukan.

### Konfirmasi setup yang siap

Pengaturan kerja harus menunjukkan pengontrol dan drive di bar status, misalnya nomor drive, ukuran, kepadatan, dan COM Port. Masuk **Opsi > Kontrol dan drive **, controller harus ditandai ** Tersedia ** dan drive ** Dikonfigurasi **Lari ** Informasi kendali** sebelum membaca media berharga jika Anda ingin memverifikasi komunikasi tanpa mengubah disk.

### Memilih mesin

GW GUI dapat mengekspos lebih dari satu implementasi untuk beberapa operasi. The **Greaseweazle Host Tools** mesin memanggil dikonfigurasi `gw.exe`; internal GW GUI mesin menangani operasi yang didukung di dalam aplikasi. Pemilihan mesin eksplisit dan independen untuk membaca, menulis, konversi, dan Disk ExplorerJika sebuah operasi tidak didukung oleh mesin yang dipilih, GW GUI laporan bahwa kondisi bukannya mengubah mesin otomatis.

## Jendela utama

Jendela utama mengelompokkan operasi utama ke tujuh tab:

- **Baca** Membuat gambar dari disk fisik.
- **Tulis** menulis gambar ke disk fisik.
- **Konversi** mengubah satu format berkas salinan diska menjadi satu atau lebih format keluaran.
- **Visualisasi** menampilkan trek dan flux atau dekode data.
- **Disk Explorer** browses sistem berkas yang didukung dan isi disk.
- **Perkakas** menyediakan perangkat keras pemeliharaan dan perintah diagnosa.
- **Emulasi** mengelola dan berjalan disimpan mesin diemulasi.

Konsol di bawah menampilkan perintah yang sedang dieksekusi dan keluaran. Batang status melaporkan drive, profil, dan keadaan saat ini.

### Membaca antarmuka

Kebanyakan halaman operasi mengikuti pola yang sama:

1. **Sumber atau tujuan** kontrol mengidentifikasi disk, image, atau folder.
2. **Kontrol format** pilih deteksi otomatis atau sebuah mesin eksplisit dan format.
3. **Pengendalian profil** aplikasikan pengaturan dapat digunakan kembali.
4. **Pengaturan tingkat lanjut** mengekspos parameter yang biasanya opsional.
5. **Jalankan** memulai operasi.
6. The **konsol** menampilkan perintah, kemajuan, peringatan, dan galat yang dihasilkan.

The **Jalankan** tombol tidak menyiratkan bahwa semua nilai aman untuk disk yang dimasukkan. Selalu tinjau tujuan dan kandar yang dipilih sebelum suatu operasi tulis atau pemeliharaan.

### Batang status dan konsol

Sisi kiri dari batang status mengidentifikasi drive fisik aktif. Pusat menunjukkan profil aktif ketika salah satu dipilih. Indikator negara melaporkan apakah aplikasi siap atau sibuk. Konsol ini tidak hanya diagnostik: ini adalah catatan otoriter dari perintah yang dikirim ke mesin terpilih. Gunakan kontrol salinan ketika Anda perlu mempertahankan atau berbagi perintah itu.

## Membaca disk

Buka **Baca** tab untuk menangkap disket disket fisik sebagai gambar.

<p align="center"><img src="../images/main-read-en.png" alt="Baca tab" width="78%"></p>

### Prosedur dasar

1. Sisipkan diska sumber pada kandar yang dikonfigurasi.
2. Pilih tipe gambar:
   - **Citra mentah (SCP)** mempertahankan informasi tingkat flux-.
   - **Format disk yang dikenal** Membuat gambar memakai mesin dan format yang dipilih.
3. Pilih folder tujuan.
4. Masukkan nama berkas keluaran.
5. Pilih profil jika diperlukan.
6. Klik **Jalankan**.

Konsol menunjukkan perintah dan kemajuan yang tepat. Jangan hapus disk atau putuskan pengontrol sampai operasi selesai.

### Memilih tipe keluaran

Gunakan **Citra mentah (SCP)** ketika tujuannya adalah archival capture, analisis, pemulihan, atau kemudian konversi. Gambar mentah mencatat informasi waktu dan beberapa revolusi, yang berguna untuk format yang tidak biasa, sektor lemah, skema perlindungan, dan media yang rusak.

Gunakan **Format disk yang dikenal** ketika Anda sudah tahu keluarga disk dan perlu langsung digunakan gambar sektor. Pilihan ini mungkin lebih kecil dan lebih mudah untuk membuka dalam perangkat lunak lain, tetapi mewakili hasil yang didekode daripada setiap detail diamati oleh drive.

Ketika tidak pasti, membuat gambar mentah pertama. Anda dapat mengkonversi nanti tanpa membaca disk lagi.

### Folder, nama berkas, dan profil

The **Folder ** adalah direktori tujuan. The ** Nama berkas** harus mengidentifikasi disk tanpa mengandalkan hanya pada label fisik. Sebuah nama archival yang berguna berisi judul, nomor disk atau sisi, dan sebuah kondisi catatan ketika dapat diterapkan. Jangan tambahkan ekstensi format yang konflik dengan format keluaran yang dipilih.

A **Profil ** berlaku sebuah set disimpan dari parameter baca. Pilih satu saja ketika Anda tahu apa isinya. The ** Baku** profil yang sesuai untuk usaha pertama yang normal; profil pemulihan khusus dapat sengaja membaca lebih banyak revolusi atau jangkauan trek yang berbeda dan karena itu butuh waktu lebih lama.

### Pengaturan tingkat lanjut

Ekspansi **Pengaturan tingkat lanjut** untuk mengakses format -spesifik atau parameter ahli. Biarkan nilai-nilai ini tidak berubah kecuali disk memerlukan suatu jangkauan trek tertentu, jumlah revolusi, atau pilihan pengendali.

Nilai lanjutan umum termasuk:

| Tatanan | Tujuan | Kapan untuk mengubahnya |
|---|---|---|
| Jangkauan trek | Batasi cylinders dan kepala untuk dibaca | Media single-sided, geometri yang tidak biasa, atau lulus pemulihan target |
| Revolusi | Kontrol berapa banyak rotasi yang diambil | Tingkatkan trek yang tidak stabil atau dilindungi; kurangi hanya untuk kecepatan bila sesuai |
| Argumen ahli | Melewati parameter mesin tambahan | Hanya ketika berikut didokumentasikan Greaseweazle bimbingan |

### Verifikasi bacaan sukses

Jangan hanya mengandalkan tidak adanya dialog kesalahan. Setelah perintah selesai:

1. Konfirmasi bahwa berkas keluaran ada dan tidak kosong.
2. Baca baris konsol akhir untuk trek yang gagal atau hilang.
3. Buka gambar dalam **Visualisasi** untuk memeriksa bahwa kedua sisi dan jangka trek diharapkan berisi data.
4. Buka di **Disk Explorer** ketika sistem berkas didukung.
5. Menjaga log operasi dengan penangkapan archival penting.

Jika diulang dibaca berbeda, melestarikan setiap penangkapan mentah daripada menimpa yang pertama. Perbedaan bisa berguna selama pemulihan.

## Menulis disk

Buka **Tulis** tab untuk menulis gambar yang ada ke disk disket fisik.

<p align="center"><img src="../images/main-write-en.png" alt="Tulis tab" width="78%"></p>

### Prosedur dasar

1. Masukkan disk tujuan.
2. Pilih gambar sumber dengan **Ramban**.
3. Konfirmasikan format yang terdeteksi.
4. Pilih profil jika diperlukan.
5. Klik **Jalankan**.

Menulis menggantikan data pada disk tujuan. Verifikasi kandar dan gambar yang dipilih sebelum dimulai.

> **Peringatan:** Menulis itu merusak. Ini menggantikan data magnetik pada disk tujuan. Gunakan write- protected source archive dan disk tujuan terpisah kapanpun mungkin.

### Sebelum menulis

Periksa empat butir sebelum mengklik **Jalankan**:

1. **Gambar:** path yang dipilih adalah image sumber yang diinginkan.
2. **Disk:** disk dalam drive dapat dengan aman ditimpa.
3. **Drive:** ukuran terkonfigurasi dan setelan kepadatan media tujuan.
4. **Format:** deteksi otomatis atau format yang dipilih secara manual cocok dengan image.

Jika gambar sumber belum diuji, buka pada **Visualisasi ** atau ** Disk Explorer** pertama. Penulisan yang sukses tidak dapat memperbaiki gambar sumber yang tidak lengkap.

### Periksa dan modifikasi trek

Setelah gambar dipilih, **Trek Visualize ** Membuka representasi trek. ** Ubah** mengekspos modifikasi gambar yang didukung sebelum menulis. Aksi tersedia bergantung pada format dan mesin yang dipilih.

### Memverifikasi diska yang ditulis

Ketika mesin mendukung verifikasi, gunakan untuk media penting. Jika tidak, baca disk yang ditulis kembali ke gambar baru dan bandingkan isi yang didekode atau memeriksanya di **Visualisasi** Jauhkan penangkapan verifikasi terpisah dari gambar asli sehingga asli tidak pernah ditimpa.

Jika menulis gagal pada trek yang konsisten, periksa kondisi disk, kepadatan, drive kebersihan, dan konfigurasi drive. Jika kegagalan terjadi secara acak, periksa USB Stabilitas dan komunikasi pengendali.

## Mengkonversi gambar disk

The **Konversi** tab mengubah citra sumber menjadi satu atau beberapa format tujuan.

<p align="center"><img src="../images/main-conversion-en.png" alt="Tab konversi" width="78%"></p>

### Prosedur dasar

1. Pilih gambar sumber.
2. Secara opsional menyediakan nama keluaran.
3. Pilih keluarga mesin.
4. Pilih satu atau lebih format keluaran dan ekstensi.
5. Aktifkan **Tambah tag** jika nama berkas harus menggunakan pola tag yang dikonfigurasi.
6. Klik **Jalankan**.

The **Dipilih ** daftar panel keluaran yang diminta. ** Migrasi berkas** menyediakan larutan kerja yang didedikasikan untuk bermigrasi berkas yang didukung ketimbang melakukan konversi image standar.

### Memilih format

The **Mesin ** tampilkan filter format yang ditampilkan di ** Format** panel. Nama format menjelaskan tata letak disk logikal; ekstensi menjelaskan wadah keluaran. Beberapa format dapat direpresentasikan oleh lebih dari satu ekstensi, dan beberapa wadah tidak dapat menyimpan setiap fitur dari sumber mentah.

Pilih hanya keluaran yang Anda butuhkan. Multiple format berguna ketika membuat master archival, sebuah salinan emulator- kompatibel, dan salinan untuk alat analisis lain dalam satu operasi.

### Penamaan keluaran dan tag

**Nama keluaran ** memungkinkan Anda mengontrol nama dasar yang dihasilkan untuk format yang dipilih. ** Tambah tag ** menerapkan pola nama berkas yang dikonfigurasi dalam ** Opsi > Umum**. Tag dapat mengkodekan keluarga, format, ekstensi, tanggal, atau waktu. Pratilik contoh dalam Opsi sebelum mengubah batch besar sehingga berkas dinamai secara konsisten.

### Memeriksa hasil konversi

Untuk setiap keluaran yang diminta:

1. Konfirmasi bahwa sebuah berkas dibuat.
2. Periksa konsol untuk trek atau sektor yang tidak dapat diterjemahkan.
3. Buka hasilnya **Disk Explorer** jika berisi sebuah sistem berkas yang didukung.
4. Bandingkan kapasitas dan isi diska yang diharapkan dengan sumber.

Sebuah konversi dapat selesai ketika melaporkan kehilangan informasi yang melekat ke format tujuan. Pertahankan gambar mentah asli bahkan ketika gambar dikonversi tampak benar.

## Memvisualisasikan suatu image disk

The **Visualisasi** tab menampilkan struktur dan distribusi data dari sebuah gambar.

<p align="center"><img src="../images/main-visualization-en.png" alt="Tab visualisasi" width="78%"></p>

1. Klik **Buka suatu image disk**.
2. Simpan **Deteksi otomatis** diaktifkan, atau pilih mesin dan format secara manual.
3. Gunakan **Perkecil sambungan** untuk menjaga kedua belah pihak pada tingkat zoom yang sama.
4. Gunakan **Reset** untuk mengembalikan pandangan awal.
5. Buka **Inspektur** untuk informasi rinci tentang wilayah yang dipilih.

Legenda membedakan aliran normal, transisi pendek dan panjang, tajuk, data yang didekode, dan terdeteksi anomali. Gambar mentah mungkin berisi data yang tidak dapat diterjemahkan ke dalam sistem berkas yang dikenal tetapi masih dapat diperiksa di sini.

### Interpreting tampilan

Setiap panel lingkaran besar mewakili satu sisi disk. Pusat mengidentifikasi sisi dan keadaan datanya saat ini; posisi konsentris sesuai dengan trek. Warna mengklasifikasikan daerah terdeteksi menurut legenda. Visualizer dimaksudkan untuk menjawab pertanyaan seperti:

- Apakah gambar berisi data di satu sisi atau keduanya?
- Apakah trek yang diharapkan hadir?
- Apakah anomali terisolasi atau diulang di disk?
- Apakah deteksi otomatis mengidentifikasi mesin dan format yang masuk akal?

Warna anomali adalah alasan untuk memeriksa wilayah, bukan bukti bahwa disk tidak dapat digunakan. Copy protection, non-standard formating, a weak recording, and a ruiled sector can product different structures that contekstual contekstual interpretasi.

### Urutan inspeksi direkomendasikan

Mulai dengan zum terkait diaktifkan untuk membandingkan kedua sisi pada skala yang sama. Pilih daerah yang mencurigakan, buka **Inspektur**(Dan demi angin yang bertiup dengan kencang) yang bertiup sangat kencang. Jika hasilnya muncul sebagai masalah deteksi, non-aktifkan deteksi otomatis dan pilih mesin dan format yang dikenal. Kembali ke deteksi otomatis setelah tes sehingga pengaturan paksa tidak sengaja digunakan untuk gambar lain.

## Menjelajahi isi diska

The **Disk Explorer** tab browses didukung image disk sebagai hirarki berkas.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer tab" width="78%"></p>

1. Buka gambar yang sudah ada atau baca disk.
2. Simpan **Deteksi otomatis** diaktifkan kecuali Anda perlu memaksa suatu mesin atau format.
3. Ulas informasi volume: sistem, perlindungan, sistem berkas, kapasitas, ruang kosong, dan jumlah item.
4. Menelusuri direktori di panel kiri.
5. Pilih objek untuk melihat rinciannya di panel kanan.

Bila format gambar atau sistem berkas tidak didukung, gunakan **Visualisasi** untuk memeriksa struktur mentah sebagai gantinya.

### Memahami panel

Ringkasan atas menggambarkan gambar yang dikait dan volume yang terdeteksi. Panel kiri-kecil berisi hirarki direktori. Tabel pusat daftar butir dalam direktori yang dipilih dengan nama, tanggal modifikasi, tipe, dan ukuran. Panel yang tepat menunjukkan rincian bagi butir yang dipilih.

Disk Explorer tidak menyiratkan bahwa setiap trek mentah telah diterjemahkan dengan sempurna. Gunakan ringkasan volume dan jumlah butir sebagai pemeriksaan ringkas cepat, kemudian buka berkas perwakilan atau membandingkannya dengan daftar direktori yang dikenal ketika akurasi pelestarian penting.

### Ketika tidak ada yang muncul

Pertama mengkonfirmasi bahwa jalur gambar benar. Kemudian periksa mesin dan format terdeteksi. Gambar yang valid mungkin berisi sistem berkas yang tidak didukung atau rusak, dalam hal ini penjelajah dapat tetap kosong meskipun **Visualisasi** Menunjukkan rekaman data. Jangan menimpa atau membuang gambar sumber hanya berdasarkan penjelajah kosong.

## Menggunakan alat

The **Perkakas** Grup tab Greaseweazle Operasi pemeliharaan.

<p align="center"><img src="../images/main-tools-en.png" alt="Tab alat" width="78%"></p>

Pilih sebuah perintah dari daftar di sebelah kiri, tinjau parameternya, kemudian klik **Jalankan** Destructive atau hardware- perintah yang berubah hanya harus digunakan setelah memverifikasi pengontrol yang dipilih dan drive.

Kebanyakan dialog alat mengandung tiga area: parameter di bagian atas, area status dan raw-output di tengah, dan perintah yang dihasilkan di bagian bawah. Perubahan pratinjau perintah sebagai opsi diaktifkan. Parameter yang belum diperiksa biasanya berarti "jangan ubah nilai ini", sedangkan parameter yang diperiksa termasuk nilai dalam perintah.

Dialog diagnostik individu digambarkan dalam [Diagnosa dan pemeliharaan Perangkat Keras](#hardware-diagnostics-and-maintenance).

## Emulasi

### Membuka mesin yang disimpan

The **Emulasi ** Daftar tab konfigurasi disimpan. Pilih satu dan klik ** Buka**Setiap mesin berjalan muncul di tab sendiri.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Layar selamat datang emulasi" width="78%"></p>

Membuat dan menyunting mesin dalam **Opsi > Emulasi > Konfigurasi ** dan ** Opsi > Emulasi > Amiga**.

Bila tak ada konfigurasi, buat satu di Opsi terlebih dahulu. Konfigurasi disimpan menggabungkan model mesin, versi emulator, ROM, memori, video, audio, penyimpanan, dan pemetaan masukan. Menyimpan konfigurasi tidak memulainya; kembali ke utama **Emulasi ** tab dan klik ** Buka**.

### Jalankan - mesin kontrol

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Menjalankan mesin yang diemulasi" width="78%"></p>

Alat menjalankan-mesin menyediakan daya, jeda, reset, save- state, load- negara, menangkap, dan menampilkan kontrol. Ini juga menunjukkan:

- konfigurasi quick- save dan quick- load jalan pintas;
- perender aktif, seperti Direct3D 11;
- layar penuh dan mouse- rilis jalan pintas;
- kondisi audio, pengontrol, dan tetikus;
- resolusi saat ini, refresh rate, dan tingkat frame.

Strip disk di bagian bawah tampilan emulasi yang dapat dilepas oleh media untuk setiap kandar yang ditata. Tugas papan tik dapat diubah dalam **Opsi > Emulasi > Pintas**, ketika diemulasi keyboard, mouse, dan pemetaan pengontrol dikonfigurasi dalam korespondensi Amiga tab.

### Referensi bilah alat

| Kontrol kelompok | Tujuan |
|---|---|
| Daya dan jeda | Mulai, berhenti, berhenti, atau resume mesin yang diemulasi |
| Reset kontrol | Melakukan aksi reset yang dikonfigurasi lembut atau keras |
| Kontrol keadaan | Simpan atau muat suatu keadaan emulator untuk kelanjutan cepat |
| Tangkap | Menyimpan gambar tampilan yang telah digandakan |
| Tampilan | Mengubah presentasi tampilan atau memasuki layar penuh |
| Pengingat keadaan cepat | Tampilkan pintasan save / load aktif |
| Perender | Laporkan backend video aktif |
| Pengingat masukan | Menampilkan layar penuh dan mouse- jalan pintas rilis |
| Indikator perangkat | Laporkan keadaan audio, pengontrol, dan tetikus |
| Penampilan | Laporan ukuran keluaran, frekuensi refresh, dan laju frame |

### Meninggalkan layar penuh atau melepaskan tetikus

toolbar menampilkan kunci yang sedang ditugaskan. Dalam konfigurasi ilustrasi, **Alt + Kembali ** ubah layar penuh dan ** F12** lepaskan tikusnya. Perlakukan nilai yang ditampilkan sebagai otoritas karena jalan pintas dapat dipindahkan.

### Menggunakan media floppy

Strip drive mengidentifikasi setiap kandar yang telah digandakan, seperti `DF0:`. Gunakan kontrol media untuk menyisipkan, mengganti, atau mengeluarkan gambar. Mengganti perubahan media hanya disk yang dimasukkan oleh mesin yang sedang berjalan; ini tidak mengubah definisi perangkat penyimpanan dalam mesin yang disimpan kecuali aksi tersebut secara eksplisit disimpan.

## Opsi aplikasi

Buka **Opsi** dari jendela utama untuk mengatur aplikasi.

### Umum

<p align="center"><img src="../images/options-general-en.png" alt="Opsi umum" width="72%"></p>

The **Umum** tab berisi:

- folder baku berkas salinan gambar:
- bahasa antar muka dan tema;
- pembuatan filename- tag untuk konversi;
- pola tag gubahan dan terdefinisi baru-baru ini;
- contoh nama berkas aktif.

Variabel tag termasuk nama sumber, keluarga, format, ekstensi, tanggal, dan waktu. Gunakan tombol reset untuk mengembalikan pola baku.

Nama berkas pratinjau pemutakhiran sebelum berkas dibuat. Gunakan untuk mendeteksi pemisah duplikasi, ekstensi hilang, atau nama ambigu. Pola gubahan kini menyediakan akses cepat ke skema penamaan sebelumnya tanpa mengganti preset yang kini.

### Log

<p align="center"><img src="../images/options-logs-en.png" alt="Opsi log" width="72%"></p>

Logging dapat dikonfigurasi secara independen untuk setiap operasi. Untuk setiap kategori, pilih apakah akan menyimpan log, set ukuran berkas maksimum, dan memutuskan apakah log sebelumnya harus ditahan. Ukuran `0` berarti tak terbatas. **Buka folder** membuka direktori log kini.

Aktifkan **Simpan log sebelumnya** untuk pelestarian dan pekerjaan diagnostik di mana sejarah beberapa upaya penting. Menonaktifkan ketika hanya hasil terbaru yang berguna. Batas ukuran maksimum berlaku untuk penyimpanan log, bukan untuk menangkap image disk.

### Kontrol dan drive

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Kontrol dan drive" width="72%"></p>

Gunakan tab ini ke:

- pemindaian untuk controller terhubung;
- tambahkan dan hapus konfigurasi drive;
- pilih ukuran kandar, kepadatan, dan kecepatan;
- simpan pengaturan perangkat keras;
- pilih atau otomatis temukan `gw.exe`;
- periksa untuk dan unduh Greaseweazle Host Tools pembaruan;
- mengembalikan path eksekusi yang telah dikonfigurasi sebelumnya.

Pengaturan perangkat keras disimpan tersedia ketika suatu drive diputus sementara.

#### Menambahkan drive

1. Klik **Pindai** dan menunggu pengontrol terhubung muncul.
2. Klik **Tambah suatu drive** jika kandar yang diperlukan belum terdaftar.
3. Pilih nomor drive logisnya, ukuran fisik, kepadatan rekaman, dan kecepatan rotasi.
4. Simpan baris.
5. Konfirmasi bahwa itu menunjukkan **Tersedia ** dan ** Dikonfigurasi**.

Gunakan kontrol sampah hanya untuk menghapus konfigurasi yang disimpan; ini tidak memutuskan perangkat keras. Jika pengontrol yang sama muncul pada berbeda COM Port kemudian, pindai lagi sebelum mengasumsikan bahwa port tersimpan masih valid.

#### Kelola Greaseweazle Host Tools

**Cari gw.exe ** Mencari lokasi yang diketahui. ** Pilih ** Memilih executable tertentu. ** Periksa pemutakhiran ** query versi yang tersedia tanpa mengganti yang terpasang. ** Unduh versi terbaru ** memasang paket yang dipilih ** Gunakan path sebelumnya ** restore lokasi konfigurasi sebelumnya. Setelah mengubah executable, jalankan ** Informasi kendali** untuk mengkonfirmasi bahwa versi yang dipilih dapat berkomunikasi dengan pengendali.

### Mesin

<p align="center"><img src="../images/options-engines-en.png" alt="Pemilihan mesin" width="72%"></p>

Pilih mesin secara independen untuk membaca, menulis, konversi, dan Disk ExplorerMesin yang dipilih dipakai secara ketat: jika tidak dapat melakukan operasi yang diminta, GW GUI Melaporkan keterbatasan bukannya beralih diam-diam mesin.

Kemerdekaan ini disengaja. Sebagai contoh, pembacaan fisik dapat menggunakan Greaseweazle Host Tools sementara konversi gambar dan eksplorasi menggunakan mesin internal. Rekam pilihan mesin dalam profil atau catatan proyek ketika reproduksi penting.

### Profil

<p align="center"><img src="../images/options-profiles-en.png" alt="Profil" width="72%"></p>

Pengaturan penyimpanan yang dapat dipakai ulang untuk operasi baca, tulis, dan konversi. Pilih kategori relevan untuk mengelola profil. Profil yang dipilih ditampilkan pada batang status jendela utama dan pada layar operasi.

Gunakan profil untuk cara kerja yang berulang daripada koleksi yang tak bisa dijelaskan dari bendera ahli. Berikan setiap profil nama tujuan-spesifik, seperti drive tertentu, keluarga disk, atau metode pemulihan. Tinjau profil setelah memperbarui mesin yang mendasari karena pilihan yang didukung dapat berubah.

## Opsi emulasi

The **Emulasi** pilihan berisi pengaturan penyimpanan umum, jalan pintas global, konfigurasi tersimpan, dan pengaturan khusus mesin.

### Folder emulasi umum

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Opsi emulasi umum" width="72%"></p>

Tata folder penyimpanan emulasi bersama dan folder baku bagi penangkapan dan status yang disimpan. **Buka folder** membuka lokasi berbagi di File Explorer.

Jauhkan penangkapan dan negara bagian tersimpan dalam folder terpisah. Penangkapan adalah gambar biasa; negara bagian yang disimpan mengandung keadaan mesin emulator- spesifik dan mungkin tergantung pada versi dan konfigurasi emulator yang membuatnya. Back up konfigurasi dan media bersama negara-negara penting yang tersimpan.

### Jalan pintas global

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Jalan pintas emulasi" width="72%"></p>

Cari suatu aksi atau penempatan kunci, assign atau hapus pintasan, pulihkan baku, dan konflik jelas. Kolom status mengidentifikasi tugas valid dan konflik.

Untuk mengubah jalan pintas, temukan aksi, klik **Atur **, dan tekan kombinasi kunci yang diinginkan. Periksa status sebelum menutup Opsi. ** Hapus konflik ** menghapus tugas yang bertentangan; ini tidak mengembalikan pemetaan baku. Gunakan ** Kembalikan default** ketika anda ingin mengganti tugas gubahan dengan set standar.

### Konfigurasi tersimpan

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Konfigurasi emulasi tersimpan" width="72%"></p>

Daftar halaman mesin yang disimpan. Pilih sebuah konfigurasi untuk mengeditnya **Amiga** tab. Anda dapat menyegarkan daftar atau menghapus konfigurasi yang dipilih.

Menghapus konfigurasi menghilangkan definisi mesin yang disimpan. Ini seharusnya tidak digunakan sebagai cara untuk mengeluarkan media atau menutup mesin yang sedang berjalan. Sebelum dihapus, catatan apapun ROM, gambar hard-disk, dan berkas keadaan yang terkait dengan konfigurasi.

## Amiga konfigurasi

Antar muka kini menyediakan rincian Amiga halaman konfigurasi. Struktur pengaturan yang sama dapat diperpanjang untuk sistem emulasi lain tanpa mengubah arus kerja utama.

### Umum

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga pengaturan umum" width="72%"></p>

Pilih Amiga model, simpan konfigurasi, instal atau ganti versi emulator, dan definisikan folder baku untuk hard disk dan media lain. **Versi pencarian** query resmi sumber emulator-versi.

Mulailah dengan model karena membatasi halaman kemudian. Mengubah itu dapat mengubah yang tersedia CPU, memori, ROM, chipset, dan pilihan penyimpanan. Setelah memilih versi emulator, simpan konfigurasi sebelum luncurkan dari jendela utama. Memasang versi emulator lain menggantikan versi yang dipakai oleh konfigurasi itu; ini tidak membuat salinan kedua dari mesin.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU pengaturan" width="72%"></p>

The CPU halaman menunjukkan prosesor yang dipilih oleh model mesin dan menyediakan presisi yang kompatibel, FPU, dan pilihan kecepatan. Opsi yang tidak diterapkan pada model yang dipilih tetap tidak aktif.

- **CPU model** Mengidentifikasi prosesor yang digandakan.
- **Presisi** mengontrol model waktunya. Mode-mode tepat mendukung kompatibilitas perangkat keras tetapi membutuhkan lebih banyak proses host.
- **FPU** memungkinkan sebuah floating-point yang kompatibel ketika didukung.
- **CPU kecepatan** Memilih waktu asli atau mode dipercepat.

Untuk sebuah konfigurasi baseline, simpan model-turunan CPU dan kecepatan asli. Ubah percepatan hanya setelah sepatu boot mesin dengan benar pada pengaturan standar.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM pengaturan" width="72%"></p>

Atur Chip RAM, Lambat RAM, Cepat RAM, dan didukung memori ekspansi. Pesan kompatibilitas menjelaskan pembatasan untuk mesin yang dipilih, dan total memori yang dikonfigurasi ditampilkan di bagian bawah.

**Chip RAM ** dapat diakses ke chip gubahan dan diperlukan oleh platform. ** Lambat RAM ** merepresentasikan memori ekspansi yang kompatibel digunakan oleh konfigurasi umum. ** Cepat RAM ** adalah processor-orientasi memori ekspansi. ** Zorro III RAM** hanya berlaku untuk model yang mendukung arsitektur ekspansi. Pesan kompabilitas dan kontrol dinonaktifkan mencegah kombinasi bahwa model yang dipilih tidak dapat mewakili.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM pengaturan" width="72%"></p>

Pilih sistem Kickstart ROM, opsional extended ROM, dan ROM kunci. Terdeteksi...ROM tampilkan nama, revisi, dan kompatibilitas dengan model yang dipilih. Pilih sebuah terdeteksi ROM dan klik **Gunakan**, atau menelusuri ke berkas secara manual.

ROM berkas tidak diberikan oleh GW GUIGunakan ROM yang diperbolehkan untuk digunakan.

Daftar terdeteksi lebih baik untuk menebak dari nama berkas: itu melaporkan ROM identitas dan revisi dan evaluasi kompatibilitas dengan model yang dipilih. **Cocok ** adalah pilihan normal; ** Sebagian kompatibel ** Menunjukkan bahwa ROM mungkin boot tetapi tidak persis sesuai mesin. ** Segarkan ** batalkan konfigurasi ROM lokasi. ** Gunakan** assign yang dipilih terdeteksi ROM ke konfigurasi.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga pengaturan video" width="72%"></p>

Atur standar video, rasio aspek, resolusi, mode baris, cropping perbatasan, renderer, kedalaman warna, membingkai, gamma, dan fixing flicker. Pengaturan chipset tambahan tersedia lebih jauh di bawah halaman ketika didukung oleh model yang dipilih.

| Tatanan | Efek praktis |
|---|---|
| Standar video | Memilih PAL atau NTSC waktu dan diharapkan perilaku refresh |
| Rasio aspek | Mengendalikan bagaimana gambar yang diemulasi skala |
| Resolusi | Memilih rincian keluaran otomatis atau eksplisit |
| Mode garis | Kontrol perawatan interlaced atau line- double output |
| Batas tanaman | Menghilangkan pemindaian overyang tidak digunakan hanya ketika diaktifkan |
| Rendering | Memilih backend grafis |
| Kedalaman warna | Memilih presisi warna keluaran |
| Frame skip | Reduces frame yang dirender ketika diaktifkan |
| Gamma | Atur respon kecerahan |
| Flicker fixer | Mode proses yang akan terlihat berkedip |

Ubah satu tampilan pengaturan pada satu waktu. Jika jendela emulasi menjadi kosong atau tidak stabil, kembali ke resolusi otomatis, frame skip dinonaktifkan, gamma netral, dan renderer sebelumnya bekerja.

### Audio

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga pengaturan audio" width="72%"></p>

Aktifkan atau matikan audio, pilih perangkat keluaran dan latensi, kemudian konfigurasi interpolasi, Amiga Penyaringan, tipe penyaring, pemisahan stereo, suara penggerak floppy-, dan volume audio CD-.

Latensi bawah mengurangi penundaan tapi dapat menyebabkan drop-out pada komputer sibuk. Tingkatkan jika audio crackles. Interpolasi dan Amiga filter audio mengubah suara reproduksi daripada meniru logika program. Mengemudikan volume suara mengontrol suara mekanis simulasi terpisah dari normal Amiga audio.

### Penyimpanan

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga pengaturan penyimpanan" width="72%"></p>

Halaman penyimpanan daftar pengidentifikasi perangkat, tipe, model, media terkait, dan aksi yang tersedia. Tambah, konfigurasi, atau hapus perangkat di sini. Floppy disk dan CD dapat dimasukkan atau diganti langsung dari mesin yang sedang berjalan.

The **identifikasi perangkat ** adalah bagaimana sistem emulasi alamat perangkat. ** Tipe ** Membedakan floppy, hard-disk, optik, dan perangkat yang didukung lainnya. ** Model ** Mendeskripsikan perangkat keras yang diemulasi, sementara ** Media yang berasosiasi** Mengidentifikasi gambar yang saat ini ditugaskan. Mengkonfigurasi perangkat sebelum menghubungkan media writable yang berharga, dan menjaga backup dari gambar hard-disk.

### Papan Ketik

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga Pengaturan papan tik" width="72%"></p>

Cari Amiga kunci dan host tugas, menetapkan kunci baru, menghapus pemetaan, mengembalikan baku, atau konflik jelas. Kolom status melaporkan apakah setiap tugas valid.

Nama kolom kiri yang ditata Amiga kunci; **Asosiasi** Menunjukkan kombinasi kunci host. Pemetaan yang valid masih dapat merepotkan jika Windows atau aplikasi mengisi jalan pintas yang sama, sehingga uji kombinasi kritis di dalam mesin yang berjalan. Hindari memberikan mouse- rilis atau layar penuh jalan pintas ke kunci bahwa perangkat lunak yang tergandakan perlu sering.

### Tetikus

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga Pengaturan tetikus" width="72%"></p>

Atur kecepatan tetikus fisik, pilih batang analog mana yang mengontrol tetikus, atur zona mati dan kecepatan analog, dan konfigurasi pemetaan aksi. Kembalikan default atau bersihkan pemetaan konflik bila diperlukan.

Meningkatkan zona mati jika pengendali menyebabkan melayang pointer. Sesuaikan kiri dan kanan-tongkat kecepatan secara independen ketika kedua tongkat diaktifkan. Tabel pemetaan bawah associate host dimasukkan dengan tombol tetikus atau tindakan; memeriksa status konflik setelah mengubah pemetaan controller di tempat lain.

### Pengontrol

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga pengaturan pengontrol" width="72%"></p>

Mendeteksi controller terhubung, perangkat assign dan tipe pengontrol ke Amiga port, and configure controller maplings and turbo- fire settings. Pilihan tersedia bergantung pada perangkat keras terdeteksi dan mesin yang dipilih.

Port 1 dan Port 2 dikonfigurasi secara independen. **Otomatis** tipe pengontrol adalah titik awal yang masuk akal, tetapi perangkat lunak mengharapkan joystick atau mouse tertentu mungkin memerlukan sebuah tipe eksplisit. Jalankan deteksi sebelum menugaskan pengendali yang baru terhubung. Turbo api berulang kali mengaktifkan masukan yang dipetakan dan harus tetap dinonaktifkan kecuali permainan atau manfaat aplikasi dari itu.

## Diagnosa dan pemeliharaan perangkat keras

Dialog ini dibuka dari **Perkakas ** tab. Setiap dialog pratilik yang dihasilkan Greaseweazle perintah. Tinjau sebelum mengklik ** Jalankan**.

### Informasi kendali

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Informasi kendali" width="62%"></p>

Tampilkan informasi yang dilaporkan oleh pengendali terpilih. Ekspansi **Keluaran mentah** ketika Anda membutuhkan lengkap perintah respon.

Gunakan ini sebagai perintah diagnostik pertama. Sebuah respon sukses menegaskan bahwa GW GUI dapat memulai perangkat Host yang dikonfigurasi yang dapat dieksekusi dan berkomunikasi dengan perangkat yang dipilih. Rekam informasi firmware dan hardware sebelum melakukan pemutakhiran.

### USB lebar bandwidth

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB lebar bandwidth" width="62%"></p>

Mengukur tersedia USB komunikasi bandwidth. Gunakan untuk mendiagnosa transfer yang tidak stabil atau tidak sesuai USB koneksi.

Tutup perangkat lunak lain memakai pengontrol sebelum pengujian. Ulangi pengukuran setelah mengubah USB Port, kabel, atau hub. Bandingkan hasil dengan kondisi yang sama daripada memperlakukan pengukuran tunggal sebagai jaminan mutlak.

### Kecepatan kandar

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Kecepatan kandar" width="62%"></p>

Mengukur kecepatan rotasi drive. Meningkatkan jumlah pengukuran ketika Anda membutuhkan hasil yang lebih mewakili.

Sebuah pengukuran tunggal adalah pemeriksaan cepat; beberapa pengukuran mengungkapkan apakah kecepatan stabil. Biarkan drive mencapai kecepatan normal sebelum menafsirkan hasilnya. Nilai yang tak terduga menunjukkan kecepatan yang dikonfigurasi salah, masalah mekanis, atau masalah pengaturan pengukuran.

### Kepala pencari

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Kepala pencari" width="62%"></p>

Pindahkan kepala drive ke silinder terpilih. **Ijinkan cylinders ekstrim ** memungkinkan posisi secara normal dibatasi, dan ** Jauhkan motor aktif** meninggalkan motor berjalan selama operasi. Gunakan posisi ekstrim hanya ketika prosedur perangkat keras secara eksplisit membutuhkan mereka.

Pencarian normal berguna untuk memastikan pergerakan kepala atau posisi sebelum diagnostik. Dengarkan dampak yang tidak normal berulang dan berhenti jika tabung yang diminta tidak pantas untuk drive. Alat ini tidak membaca atau memvalidasi data di silinder tujuan.

### Diagnosa perataan kandar

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Diagnosa perataan kandar" width="62%"></p>

Berjalan berulang membaca untuk driving-alignment analisis. Ini mendukung pemilihan trek, revolusi dan jumlah baca, format decoding, flux mentah, indeks, kecepatan, PLL, density- pin, sektor hard-, TG43, dan reverse- pilihan data. Penyesuaian pekerjaan memerlukan referensi yang sesuai media dan pengetahuan hardware.

Dimulai dengan disk referensi yang dikenal dan set terkecil overrides. **Memutar trek ** mendefinisikan trek dan kepala sampel; ** Resolusi per trek ** kontrol setiap durasi contoh; ** Jumlah bacaan** Tentukan pengulangan. Aktifkan definisi disk suai atau format pengkodean hanya ketika cocok dengan media referensi. Pilihan seperti indeks palsu, sektor keras, PLL overrides, densitas pin, dan TG43 adalah hardware- atau format -spesifik dan dapat membatalkan perbandingan ketika digunakan secara tidak benar.

### Pin hardware

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Pin hardware" width="62%"></p>

Pembaca atau perubahan pin pengendali yang didukung. Pilih pin, aktifkan **Ubah pin ** hanya ketika menulis nilai, dan pilih ** Tingkat tinggi** ketika dibutuhkan oleh operasi perangkat keras yang dimaksudkan.

Dengan **Ubah pin** dinonaktifkan, queries perintah pin. Ini yang paling aman. Mengubah tingkat secara langsung mempengaruhi pengontrol I / O dan hanya harus dilakukan dengan benar Greaseweazle dokumentasi perangkat keras dan melampirkan kabel drive.

### Reset pengontrol

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Reset pengontrol" width="62%"></p>

Reset Greaseweazle Pengontrol. Gunakan ini ketika pengontrol terdeteksi tapi tidak lagi merespon secara normal.

Tunggu untuk operasi diska yang aktif untuk selesai sebelum pengaturan ulang. Setelah itu, pindai pengontrol lagi jika status koneksinya tidak pulih secara otomatis. Sebuah reset tidak memperbaiki yang salah `gw.exe` path atau sebuah diputus USB alat.

### Puja

<p align="center"><img src="../images/tool-delays-en.png" alt="Pengontrol penundaan" width="62%"></p>

Membaca atau mengubah nilai pengontrol waktu, termasuk pilihan, langkah kepala, menetap, motor, pilihan penurunan otomatis, menulis waktu, dan indeks penundaan topeng. Aktifkan hanya nilai yang ingin Anda ubah.

Ruas tak tercontreng meninggalkan nilai pengontrol yang sesuai tak berubah. Sebelum mengedit, rekam nilai yang ada. Perubahan waktu dapat mempengaruhi setiap operasi fisik berikutnya, jadi uji dengan media yang dibuang dan kembalikan informasi-nilai baik jika perilaku menjadi tidak dapat diandalkan.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Pemutakhiran firmware" width="62%"></p>

Pemutakhiran pengontrol firmware. **Mutakhirkan pemuat boot** secara eksplisit ditandai sebagai berisiko dan harus tetap dinonaktifkan kecuali prosedur firmware resmi membutuhkannya. Jangan putuskan pengontrol selama pemutakhiran.

Sebelum memperbarui, konfirmasikan pengontrol terhubung dengan **Informasi kendali**, gunakan langsung stabil USB koneksi, dan menutup perangkat lunak lain yang bisa mengaksesnya. Setelah selesai, sambung ulang atau pindai ulang pengontrol dan baca lagi informasinya untuk memverifikasi versi firmware yang dilaporkan.

## Log dan riwayat operasi

Buka riwayat operasi untuk memeriksa log yang disimpan oleh operasi.

<p align="center"><img src="../images/operation-history-en.png" alt="Riwayat operasi" width="68%"></p>

Pilih log di sebelah kiri untuk menampilkan isinya. **Ekspor** Simpan salinan untuk diagnosa atau dukungan. Path dan baris perintah mungkin berisi nama folder pribadi, jadi tinjau log yang diekspor sebelum berbagi.

Konsol aktif di jendela utama menampilkan perintah saat ini dan keluaran terbaru. Tombol salin menyalin teks yang ditampilkan.

### Membaca log

Log diagnostik yang berguna berisi perintah, penanda waktu, keluaran mesin, dan status akhir. Bekerja dari bawah ke atas: mengidentifikasi kesalahan akhir, kemudian mencari peringatan pertama atau gagal trek yang mendahului itu. Kegagalan generik berikutnya sering hanya konsekuensi dari pesan sebelumnya, lebih spesifik.

Ketika membandingkan dua upaya, periksa bahwa pengendali, penggerak, mesin, profil, jalur sumber, format keluaran, dan argumen ahli identik. Jika tidak, hasil yang berbeda mungkin mencerminkan perubahan pengaturan daripada ketidakstabilan disk.

## Data aplikasi dan penggunaan portabel

GW GUI menjaga data pengguna terpisah dari binari aplikasi. Tergantung pada paket dan mode yang dipilih, pengaturan, log, perangkat yang diunduh, komponen emulator, menangkap, menyatakan, dan konfigurasi mesin disimpan baik dalam aplikasi `Data` direktori atau di lokasi terkonfigurasi data.

Sebelum mengganti atau memindahkan instalasi portabel, simpan folder aplikasi lengkap bersama dan backup up `Data` folder. Jangan pindahkan berkas individu dari `lib`, karena aplikasi menyelesaikan sendiri dan ketiga perpustakaan partai dari struktur itu.

### Isi backup yang disarankan

Back up the following when they is important to your workflow:

- pengaturan aplikasi dan profil;
- pengontrol dan definisi drive;
- konfigurasi emulasi;
- ROM jalan dan secara hukum dipegang ROM Backup;
- hard-disk dan removable-media image;
- ditangkap dan disimpan negara;
- catatan operasi digunakan sebagai catatan pelestarian.

Image cakram mungkin jauh lebih besar dari pengaturan. Simpan master archival baca-hanya ketika mungkin, dan bekerja pada salinan.

## Dianjurkan mengalir kerja

### Mengarsip disk yang tak dikenal

1. Inspeksi dan bersihkan drive menggunakan prosedur perawatan yang sesuai.
2. Write- melindungi disk jika mungkin.
3. Pilih **Baca > Citra mentah (SCP)**.
4. Gunakan nama berkas deskriptif dan baca kisaran trek normal dengan revolusi ganda.
5. Tinjau konsol dan log tersimpan.
6. Inspeksi kedua sisi di **Visualisasi**.
7. Mengubah salinan ke format sektor yang mungkin.
8. Uji salinan yang diubah dalam **Disk Explorer** atau software yang cocok.
9. Menjaga master mentah, log, dan catatan bersama-sama.

### Mengembalikan diska dari gambar

1. Inspeksi gambar dan konfirmasi yang diharapkan keluarga dan format.
2. Masukkan disk yang dapat ditulisi atau dengan sengaja dari ukuran dan kepadatan yang benar.
3. Buka **Tulis** dan pilih image.
4. Konfirmasikan format kandar yang dikonfigurasi dan terdeteksi.
5. Tulis disknya.
6. Membacanya kembali ke gambar verifikasi terpisah.
7. Bandingkan isi dan tinjau trek yang mencurigakan secara visual.

### Membuat suatu emulasi Amiga

1. Buka **Opsi > Emulasi > Konfigurasi** dan membuat atau memilih mesin.
2. Masuk **Amiga > Umum**, pilih model dan versi emulator.
3. Atur kompatibel, sah diperoleh ROM.
4. Pertahankan standar model CPU dan RAM pada boot pertama.
5. Atur video dan audio dengan pengaturan otomatis konservatif.
6. Tambahkan perangkat penyimpanan dan associate menyalin gambar media.
7. Tinjau papan tik, tetikus, dan pengontrol tugas.
8. Simpan konfigurasi.
9. Kembali ke **Emulasi **, pilih, dan klik ** Buka**.
10. Hanya setelah boot baseline sukses, perubahan akselerasi atau pengaturan maju satu per satu.

## Daftar cek keselamatan

Sebelum **Baca**:

- diska sumber berada dalam kandar yang benar;
- sumber ditulis - dilindungi dimana mungkin;
- path keluaran tidak akan menimpa master yang ada;
- profil dan jangkauan trek cocok dengan disk.

Sebelum **Tulis ** atau ** Hapus**:

- diska tujuan dapat dihancurkan;
- gambar dan drive yang benar;
- ukuran dan kepadatan disk kompatibel;
- tidak ada archival master sedang digunakan sebagai tujuan.

Sebelum alat yang berubah-keras:

- tidak ada operasi lain yang berjalan;
- pengontrol yang benar dipilih;
- nilai saat ini telah direkam;
- Pengontrol memiliki kekuatan yang stabil dan USB konektivitas;
- aksi didukung oleh dokumentasi perangkat keras.

## Penelusuran masalah

### Pengontrol tidak terdaftar

1. Reconnect controller langsung ke komputer.
2. Buka **Opsi > Kontrol dan drive**.
3. Klik **Pindai**.
4. Verifikasi status pengontrol dan konfigurasi drive.
5. Lari **Informasi kendali** jika deteksi berhasil tapi perintah gagal.

Jika masih tidak muncul, mencoba langsung lain USB Port dan kabel, kemudian memindai ulang. Periksa Manajer Perangkat Windows untuk perangkat serial yang baru terdeteksi. Pengontrol terlihat ke Windows tetapi tidak hadir dari GW GUI biasanya menunjuk ke port, konfigurasi, basi, atau masalah Host Tools; seorang pengendali tidak hadir dari Windows point ke USB, Power, driver, atau hardware.

### `gw.exe` tidak ditemukan

Buka **Opsi > Kontrol dan drive **, lalu gunakan ** Cari gw.exe **, ** Pilih **, atau ** Unduh versi terbaru**Konfirmasikan bahwa terdeteksi titik jalan ke yang dimaksudkan Greaseweazle instalasi.

Setelah memilihnya, lari **Informasi kendali** Jika itu gagal sebelum menghubungi perangkat keras, periksa log untuk jalur eksekusi, berkas hilang, atau versi yang tak bisa dimulai.

### Sebuah operasi menggunakan mesin yang salah

Buka **Opsi > Mesin** dan memeriksa mesin yang ditugaskan untuk operasi yang tepat. GW GUI tidak diam-diam jatuh kembali ke mesin lain.

Pengaturan mesin terpisah: mengubah mesin konversi tidak mengubah membaca, menulis, atau Disk Explorer. Membuka kembali operasi gagal setelah menyimpan opsi dan mengkonfirmasi perintah yang dihasilkan dalam konsol.

### Sebuah image tidak dikenal

Nonaktifkan deteksi otomatis hanya jika Anda tahu mesin dan format yang benar. Jika tidak, cobalah **Visualisasi** tab untuk memeriksa gambar pada tingkat yang lebih rendah.

Periksa apakah sumber adalah penangkapan flux mentah, gambar sektor, wadah terkompresi, atau berkas yang tidak berhubungan dengan ekstensi menyesatkan. Jangan pernah mengubah nama sebuah ekstensi hanya untuk memaksa deteksi; konversi harus menafsirkan struktur sumber dengan benar.

### Emulasi tidak dimulai

Verifikasi konfigurasi tersimpan, versi emulator terpasang, terpilih ROM, jalur penyimpanan, dan kompatibilitas model. Tinjau log aplikasi untuk rincian galat lengkap.

Sementara kembali CPU, RAM, video, dan penyimpanan ke sebuah model sederhana yang kompatibel baseline. Jika baseline dimulai, mengembalikan satu pengaturan custom pada satu waktu. Sebuah keadaan yang disimpan dibuat dengan versi emulator lain atau definisi mesin mungkin juga gagal bahkan ketika boot bersih bekerja.

### Jalan pintas atau masukan tidak bekerja

Periksa kedua global **Emulasi > Pintas** halaman dan mesin - spesifik keyboard, mouse, atau halaman controller. Selesaikan tugas yang ditandai sebagai konflik.

Bila tetikus ditangkap, gunakan pintasan rilis yang ditampilkan dalam bilah alat mesin. Bila suatu pengontrol tersambung setelah Opsi dibuka, jalankan pendeteksi pengontrol lagi sebelum memasukkannya.

### Sebuah perintah gagal tiba-tiba

1. Baca keluaran konsol langsung.
2. Buka **Riwayat operasi** untuk log lengkap yang disimpan.
3. Konfirmasi pengontrol, kandar, profil, mesin, dan jalur berkas.
4. Ekspor log relevan jika perlu dibagikan untuk diagnosis.

### crackles audio atau jeda

Naikkan latensi audio emulasi, tutup CPUAplikasi-aplikasi intensif, dan bingkai video kembali dilewati dan percepatan ke nilai sebelumnya. Verifikasi bahwa perangkat audio Windows yang dimaksud dipilih. Ubah satu pengaturan pada satu waktu sehingga koreksi efektif dapat diidentifikasi.

### Tampilan emulasi kosong atau lambat

Return resolusi dan mode baris ke **Otomatis**, non-aktifkan frame skipping dan flicker fix sementara, dan coba perender kerja sebelumnya. Konfirmasi bahwa konfigurasi ROM dan masukan boot media valid. The FPS indikator membantu membedakan masalah rendering-kinerja dari mesin yang tidak boot.

### Sebuah trek yang dibaca tidak stabil

Ulangi pembacaan ke nama berkas baru, tingkatkan revolusi yang sesuai, dan bandingkan trek yang terpengaruh. Bersihkan kepala drive menggunakan prosedur yang benar dan memeriksa disk untuk kerusakan fisik. Jangan berkali-kali membaca sekilas selubung atau media rusak, karena lebih lanjut lewat mungkin memperburuk itu.

## Glossary

| Masa | Artinya dalam GW GUI |
|---|---|
| Controller | The Greaseweazle antar muka perangkat keras tersambung USB |
| Kandar | Kandar floppy fisik melekat ke controller |
| Mesin | Aplikasi yang dipilih untuk menjalankan suatu operasi |
| Flux | Informasi waktu mewakili transisi magnetik yang dibaca dari disk |
| Citra mentah | Sebuah penangkapan mempertahankan informasi disk tingkat rendah, seperti SCP |
| Citra sektor | Sebuah representasi didekode terorganisasi ke sektor logis |
| Revolusi | Satu rotasi lengkap diambil ketika membaca trek |
| Cylinder | Sebuah posisi kepala radial; satu silinder dapat berisi trek di setiap sisi |
| Kepala | Sisi disk yang dipilih oleh kandar fisik |
| Profil | Suatu set pengaturan yang dapat dipakai ulang bagi suatu operasi |
| ROM | Gambar firmware yang diperlukan oleh mesin yang terukur |
| Keadaan tersimpan | Sebuah snapshot dari menjalankan status mesin emulator |
| Perender | Backend grafis yang dipakai untuk menampilkan keluaran emulasi |

## Referensi cepat

| Jika kau mau... | Pergi ke... |
|---|---|
| Mempertahankan disk fisik | **Baca** |
| Pasang gambar kembali pada disk | **Tulis** |
| Menghasilkan format gambar lain | **Konversi** |
| Inspeksi trek atau anomali flux | **Visualisasi** |
| Ramban berkas di dalam gambar | **Disk Explorer** |
| Periksa komunikasi pengendali | **Perkakas > Informasi kendali** |
| Mengukur rotasi drive | **Perkakas > Kecepatan kandar** |
| Tinjau perintah masa lalu | **Riwayat operasi** |
| Atur perangkat keras | **Opsi > Kontrol dan drive** |
| Pilih implementasi | **Opsi > Mesin** |
| Membuat atau menyunting suatu mesin yang telah diemulasi | **Opsi > Emulasi** |
| Mulai mesin yang disimpan | **Emulasi** |
