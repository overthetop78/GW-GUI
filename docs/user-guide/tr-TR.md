# GW GUI Kullanıcı Kılavuzu

GW GUI Okumak, yazmak, dönüştürmek, denetlemek ve floppy-disk görüntüleri taklit etmek için bir Windows uygulamasıdır. Kontrol edebilir Greaseweazle Donanım, iç motoru aracılığıyla disk görüntü dosyaları ile çalışır ve emated-makine konfigürasyonları çalıştırın.

Bu kılavuz, uygulamanın mevcut sürümünde gösterilen İngilizce arayüzü açıklar. Baskılanabilir kullanıcı kılavuzunun kaynağı olarak yazılır: ekran görüntüleri kontrolleri gösterirken, çevreleyen metin neyi seçeceğini, neden seçmeyi ve sonucu nasıl doğrulamayı önerir.

> **Önemli:** Bir disk okumak tahrip edici değildir. Yazma, çağlar, bellek güncelleme ve bazı donanım araçları medya veya donanım değiştirebilir. Daha önce ilgili prosedüre bağlı uyarıyı okuyun ** Execute**.

### Bu kılavuzu nasıl kullanılır

Eğer bu ilk kez kullanımınız GW GUITamam, tamam [Başlanmaya başladı](#getting-started)Sonra takip et [Bir disk okuma](#reading-a-disk). Uygulama zaten yapılandırılmışsa, gerçekleştirmek istediğiniz operasyon için doğrudan bölüme gidin. Seçeneklerin bölümleri, bir prosedür bir sürücü, motor, profil veya emated-makine ayarını değiştirmenizi istediğinizde referans olarak hizmet eder.

Interface isimleri gösterilir **cesur cesur cesur cesur** Dosya isimleri, yollar, komutlar ve gerçek değerler olarak gösterilir `code`Notlar normal davranışları açıklar; bir disk, kontrolör veya depolama yapılandırmasını değiştirebilecek işlemleri tanımlar.

## İçerikler

1. [İş akışını anlamak](#understanding-the-workflow)
2. [Başlanmaya başladı](#getting-started)
3. [Ana pencere](#main-window)
4. [Bir disk okuma](#reading-a-disk)
5. [Bir disk yazmak](#writing-a-disk)
6. [Disk görüntülerini dönüştürmek](#converting-disk-images)
7. [Bir disk görüntüsü görselleştirme](#visualizing-a-disk-image)
8. [Açıklama: Exploring disk Content](#exploring-disk-contents)
9. [araçları kullanarak](#using-the-tools)
10. [Emulation](#emulation)
11. [Uygulama seçenekleri](#application-options)
12. [Emulation seçenekleri](#emulation-options)
13. [Amiga yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma](#amiga-configuration)
14. [Donanım tanı ve bakım](#hardware-diagnostics-and-maintenance)
15. [Logs and operation history](#logs-and-operation-history)
16. [Uygulama verileri ve portatif kullanım](#application-data-and-portable-use)
17. [Önerilen akışlar](#recommended-workflows)
18. [Güvenlik kontrol listesi](#safety-checklist)
19. [Sorun Giderme](#troubleshooting)
20. [Parlak](#glossary)
21. [Hızlı referans](#quick-reference)

## İş akışını anlamak

GW GUI Görüntü-file operasyonlarının fiziksel-disk işlemleri:

| Goal | Giriş giriş | Çıktı Çıktı Çıktı | Önerilen sayfa |
|---|---|---|---|
| Bir floppy diski koruma | Fiziksel disk | Görüntü dosyası | **Read Oku** |
| Bir floppy disk | Görüntü dosyası | Fiziksel disk | **Write Write Write Write** |
| Change image format format | Görüntü dosyası | Bir veya daha fazla görüntü dosyaları | **Dönüşüm Dönüşüm Dönüşümü** |
| Inspect parçaları ve anomalileri | Görüntü dosyası | Görsel analiz | **Görselleştirme** |
| Bir görüntüde saklanan dosyalar | Desteklenen imaj/file sistemi | Dosyalar ve yönetmenler | **Disk Explorer** |
| Bir sürücü veya kontrolör | Greaseweazle Donanım donanımı | Ölçmeler veya Durum | **Araçlar** |
| Kurtarılmış bir sanal makine | Saved makine yapılandırma | Emulation session | **Emulation** |

Koruma için, önce bir ham yakalama yapın ve bir usta olarak değişmesini sağlayın. Bu ustadan dönüştürülmüş veya tamir edilmiş çalışma kopyaları oluşturun. Bu, fiziksel bir okumayı tekrarlamak ve bir sektör tabanlı formatın muhafaza edemeyeceği bilgileri korumaktan kaçınır.

## Başlanmaya başladı

### Gereksinimler

- Windows ile birlikte Microsoft .NET Uygulama tarafından gerekli olan Masaüstü Runtime.
- A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A Greaseweazle Fiziksel floppy-disk operasyonları için kontrol.
- yapılandırılmış bir yol için `gw.exe` Ne zaman kullanırken Greaseweazle Host Tools Motor.
- Yasal olarak elde edildi ROM Bir emated makine onları gerektirdiğinde dosyalar.

Uygulama, başlangıçta gerekli .NET runtime kontrol eder. Eğer eksikse, yüklemeyi takip edin, sonra yeniden başlayın GW GUI.

### Donanımı bağlamadan önce

Bir fiziksel-disk operasyonu çalıştırmadan önce aşağıdakileri kontrol edin:

1. Connect the Connect the Greaseweazle Bir stabilizatöre USB port.
2. Doğru yönelimle floppy kabloyu bağlayın.
3. Değerli medyayı eklemeden önce sürücü gücünü tedarik edin.
4. Sürücü boyutunu ve yoğunluk diski eşleştirdiğini onaylayın.
5. Mümkün olduğunda kaynak diskini yazın.

GW GUI Yanlış taksileme, uygun olmayan güç veya mekanik olarak güvenli olmayan bir sürücü tarafından kaynaklanan zararları engelleyebilir. İlk önce gerçekleştirilmiş bir diskle yabancı donanım test edin.

### İlk başlangıç

1. Open Open Open Open `gwgui.exe`.
2. Open Open Open Open **Seçenekleri**.
3. İçinde In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In **kontrolörler ve sürücüler** Kontrol için tarama ve sürücüyü yapılandırın.
4. Verify veya yolu seçin `gw.exe`.
5. İçinde In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In **Motorlar** Ancak hangi motorun her operasyonu gerçekleştirmesi gerektiğini seçin.
6. Ana pencereye dön ve gerekli işlem sekmesini seçin.

### Bu yüklemenin hazır olduğunu onaylayın

Bir çalışma kurulumu, örneğin bir sürücü numarası, büyüklüğü, yoğunluğu ve yoğunluğu ve yükseklikleri göstermelidir. COM port. İçinde In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In **Seçenekler > kontrolörler ve sürücüler ** Ancak kontrolör işaretlenmelidir **Mevcut kullanılabilir ** Ve sürücü ** Configured **. Run ** Controller bilgi** Değerli medyayı okumadan önce, bir disk değiştirmeden iletişimi doğrulamak istiyorsanız.

### Bir motor seçmek

GW GUI Bazı operasyonlar için birden fazla uygulama ortaya çıkabilir. The The The The The The The The **Greaseweazle Host Tools** Motorlar, yapılandırılan çağrıyı çağıran motorlar `gw.exe`; iç GW GUI Motor uygulama içindeki işlemleri destekledi. Motor seçimi okuma, yazma, dönüşüm ve yazma için açık ve bağımsızdır. Disk ExplorerBir operasyon seçilmiş motor tarafından desteklenmezse, GW GUI Motorları otomatik olarak değiştirmek yerine bu durumu bildiriyor.

## Ana pencere

Ana pencere, ana işlemleri yedi sekmeye dönüştürür:

- **Read Oku** Fiziksel bir diskten bir görüntü oluşturur.
- **Write Write Write Write** Fiziksel bir diske bir görüntü yazın.
- **Dönüşüm Dönüşüm Dönüşümü** Bir disk görüntü formatı bir veya daha fazla çıkış formatlarına dönüştürür.
- **Görselleştirme** izler ve flux veya kodlanmış veriler gösterir.
- **Disk Explorer** Desteklenen dosya sistemleri ve disk içerikleri.
- **Araçlar** Donanım bakımı ve teşhis komutları sağlar.
- **Emulation** Yönetilir ve kurtarılmış makineler çalışır.

En alttaki konsol, komutanın infaz edilmesini ve üretimini gösterir. Durum bar, seçilen sürücü, profil ve mevcut durumu bildiriyor.

### arayüzü okumak

Çoğu işlem sayfaları aynı modeli takip eder:

1. **Kaynak veya hedef** Kontroller disk, görüntü veya klasörü tanımlar.
2. **Biçim kontrol kontrolleri** Otomatik algılama veya açık bir makine ve format seçin.
3. **Profil kontrolleri** Yeniden kullanılabilir ayarlar uygulayın.
4. **Gelişmiş ayarlar** Normalde tercih edilen parametreler ortaya çıkar.
5. **Execute** Operasyon başlar.
6. The The The The The The The The **konsol konsol konsol** Oluşturulan komut, ilerleme, uyarılar ve hatalar gösterir.

The The The The The The The The **Execute** düğme, tüm değerlerin ek disk için güvenli olduğu anlamına gelmez. Her zaman varış noktasını gözden geçirin ve bir yazı veya bakım işleminden önce seçilmiş sürüş.

### Durum bar ve konsol

Durum çubuğunun sol tarafında aktif fiziksel sürücüyü tanımlar. Merkez, biri seçildiğinde aktif profili gösterir. Durum göstergesi, uygulamanın hazır veya meşgul olup olmadığını bildiriyor. Konsol sadece tanınmıyor: seçilen motora gönderilen komutun yazarlı kaydı. Bu komutu korumak veya paylaşmanız gerektiğinde kopya kontrolünü kullanın.

## Bir disk okuma

Açıklayın **Read Oku** Bir görüntü olarak fiziksel bir floppy disk yakalamak için sekme.

<p align="center"><img src="images/main-read-en.png" alt="Read sekmesi" width="78%"></p>

### Temel prosedür

1. Kaynak diski yapılandırılan sürücüde ekleyin.
2. Görüntü türünü seçin:
   - **Raw image (İngilizce).SCP)** flux seviyesi bilgilerini korur.
   - **Bilinen disk formatı** Seçilen bir makine ve format kullanarak bir görüntü yaratır.
3. Hedef klasörü seçin.
4. Çıktı dosyası adı girin.
5. Gerekirse bir profil seçin.
6. Click Click Click Click Click **Execute**.

Konsol tam komut ve ilerleme gösterir. İşlem bitinceye kadar diski ya da kontrol etmeyin.

### Çıktı tipini seçmek

Use Use Use Use Use **Raw image (İngilizce).SCP)** Objektif yakalama, analiz, kurtarma veya daha sonra dönüşüm olduğunda. Tehlikeli formatlar, zayıf sektörler, koruma programları ve hasarlı medya için yararlı olan bir ham görüntü kayıtları zamanlama bilgileri ve çoklu devrimler.

Use Use Use Use Use **Bilinen disk formatı** Disk ailesini zaten bildiğinizde ve doğrudan kullanılabilir bir sektör imajına ihtiyacınız var. Bu seçim diğer yazılımlarda açılmak için daha küçük ve daha kolay olabilir, ancak sürücü tarafından gözlemlenen her detaydan ziyade kodlanmış sonucu temsil eder.

belirsiz olduğunda, ilk önce çiğ görüntü oluşturabilir. Daha sonra diski tekrar okumadan dönüştürebilirsiniz.

### Folder, dosya adı ve profil

The The The The The The The The **Folder ** Hedef rehberidir. The The The The The The The The ** Dosya adı** Diski sadece fiziksel etiketine güvenmeksizin tanımlamalıdır. Yararlı bir arşiv adı başlığı, disk numarası veya tarafı içeriyor ve uygulanabilir olduğunda bir durum notu. Seçilmiş çıkış formatı ile çatışmaların bir format uzatmasını eklemeyin.

A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A A **Profil Profili ** Kurtarılan bir dizi okuma parametresi uygulanır. Sadece ne olduğunu bildiğinizde birini seçin. The The The The The The The ** Tembel** Profil normal bir ilk deneme için uygundur; özelleştirilmiş bir kurtarma profili kasıtlı olarak daha fazla devrim veya farklı bir takip aralığı okuyabilir ve bu nedenle daha uzun sürebilir.

### Gelişmiş ayarlar

Genişletilmiş Genişleme **Gelişmiş ayarlar** Formata özel veya uzman parametrelerine erişmek. Bu değerleri disk belirli bir pist aralığı, devrim sayımı veya kontrol seçeneği gerektirdiği sürece değişmeden bırakın.

Ortak gelişmiş değerler şunlardır:

| ayar ayarı | Amaç | Ne zaman değişecektir |
|---|---|---|
| Track range range | Silindirleri ve kafaları okumak | Tek taraflı medya, olağandışı geometri veya hedefli bir kurtarma geçişi |
| Devrimler | Kaç rotasyon örneği nasıl kontrol edilir | istikrarsız veya korumalı parçalar için artış; uygun olduğunda sadece hız için azaltır |
| Uzman argümanlar | Ekstra motor parametreleri | Sadece belgelendikten sonra Greaseweazle rehberlik |

### Başarılı bir okuma

Sadece bir hata diyalogunun yokluğuna güvenmeyin. Komutan tamamlandıktan sonra:

1. Çıktı dosyasının var olduğunu ve boş olmadığını onaylayın.
2. Başarısız veya eksik parçalar için son konsol hatları okuyun.
3. Görüntüyü açıklayın **Görselleştirme** Her iki tarafın ve beklenen pist aralığının veri içerdiğini kontrol etmek.
4. Açıklayın **Disk Explorer** Dosya sistemi desteklenmekte olduğunda.
5. Operasyon logunu önemli arşiv yakalamalarla tutun.

Tekrarlanan okursa, her bir ham yakalamayı ilki yazmak yerine korur. Farklılıklar kurtarma sırasında yararlı olabilir.

## Bir disk yazmak

Açıklayın **Write Write Write Write** Mevcut bir görüntüyü fiziksel bir diske yazmak için sekme.

<p align="center"><img src="images/main-write-en.png" alt="Yaz" width="78%"></p>

### Temel prosedür

1. Hedef diski açın.
2. Kaynak imajını kullanarak seçin **Göze Göz**.
3. Tespit edilen formatı onaylayın.
4. Gerekirse bir profil seçin.
5. Click Click Click Click Click **Execute**.

Yazı, hedef diskteki verileri değiştirir. Başlamadan önce seçilmiş sürüş ve görüntüyü doğrulayın.

> **Uyarı:** Yazma yıkıcıdır. Hedef diskteki manyetik verileri değiştirir. Mümkün olduğunda bir yaz korumalı kaynak arşivi ve ayrı bir hedef disk kullanın.

### Yazmadan önce

Giriş yapmadan önce dört öğe kontrol edin **Execute**:

1. **Resim:** Seçilen yol, amaçlanan kaynak görüntüsüdür.
2. **Disk:** Sürücüdeki disk güvenle yazılabilir.
3. **Drive:** Yapısal boyut ve yoğunluk hedef ortağa uygundur.
4. **Biçim:** Otomatik algılama veya manuel seçilmiş format görüntüyü eşleştirir.

Kaynak görüntüsü test edilmediyse, bunu açıklayın **Görselleştirme ** veya ** Disk Explorer** İlk olarak. Başarılı bir yazı eksik bir kaynak imajını onaramaz.

### Track denetimi ve modifikasyon

Bir görüntü seçildikten sonra, **Görselleştirme parçaları ** İzleme temsilini açar. ** Modify** Yazmadan önce desteklenen görüntü değişiklikleri ortaya koyar. Mevcut eylemler seçilmiş format ve motora bağlıdır.

### Bir yazılı diski Doğrulama

Motor doğrulamayı desteklediğinde, önemli medya için kullanın. Aksi takdirde, yazılı diski yeni bir görüntüye geri okuyun ve kodlanmış içerikleri karşılaştırın veya onu kontrol edin **Görselleştirme**. Doğrulama orijinal görüntüden ayrı tut, böylece orijinal asla yazılmamıştır.

Eğer yazı tutarlı parçalarda başarısız olursa, disk durumunu kontrol edin, yoğunluk, temizlik yapar ve konfigürasyon yapılandırması. Başarısızlık rastgele gerçekleşirse, kontrol edin USB Stabil and kontrolör iletişim.

## Disk görüntülerini dönüştürmek

The The The The The The The The **Dönüşüm Dönüşüm Dönüşümü** sekme bir kaynak görüntüyü bir veya birkaç hedef biçimine dönüştürür.

<p align="center"><img src="images/main-conversion-en.png" alt="Dönüşüm sekmesi" width="78%"></p>

### Temel prosedür

1. Kaynak imajını seçin.
2. Seçmeli olarak çıkış isimleri sağlar.
3. Bir makine ailesi seçin.
4. Bir veya daha fazla çıkış formatlarını ve uzantıları seçin.
5. Enable **Add tags** Eğer dosya isimleri yapısal etiket modelini kullanmalıdır.
6. Click Click Click Click Click **Execute**.

The The The The The The The The **Seçilmiş seçilmiş ** Panel talep edilen çıktıları listeler. ** Dosya göçü** Standart bir görüntü dönüştürme yapmak yerine migrating destekli dosyalar için özel iş akışı sağlar.

### Biçimleri seçin

The The The The The The The The **Machine Machine Machine ** Liste, gösterilen formatları filtreliyor ** Format Format** Panel. Bir format adı mantıksal disk düzeni açıklar; uzatma çıktı konteynerini açıklar. Bazı formatlar bir uzatmadan daha fazla temsil edilebilir ve bazı konteynerler bir ham kaynağın her özelliğini koruyabilir.

Aslında ihtiyacınız olan tek çıkışları seçin. Birden çok format, bir arşiv ustası oluşturmakta, bir emülatör uyumlu kopya oluşturmakta ve bir operasyonda başka bir analiz aracı için bir kopyadır.

### Çıktı adı ve etiketler

**Çıktı isimleri ** Seçilen formatlar için üretilen temel isimleri kontrol etmenize izin verin. ** Add tags ** Dosya adı kalıbında yapılandırılan ** Seçenekler > General General General General General General General General General General General General**. Tags, format, uzatma, tarih veya zaman. Örnekleri büyük bir toplu dönüştürmeden önce, böylece dosyalar sürekli olarak adlandırılır.

### Dönüşüm sonuçlarını kontrol edin

Her talep edilen çıktı için:

1. Bir dosyanın yaratıldığını onaylayın.
2. Kod veya sektörler için konsolu kontrol edin, kodlanamaz.
3. Sonuç açın **Disk Explorer** Desteklenen bir dosya sistemi içeriyorsa.
4. Beklenen disk kapasitesi ve içeriği kaynakla karşılaştırın.

Bir dönüşüm, varış formatına ait olan bilgi kaybı rapor ederken tamamlanabilir. Retain the original raw image even when the dönüştürülmüş görüntü doğru görünüyor.

## Bir disk görüntüsü görselleştirme

The The The The The The The The **Görselleştirme** sekme, bir görüntünün yapısını ve veri dağıtımını gösterir.

<p align="center"><img src="images/main-visualization-en.png" alt="Görselleştirme sekmesi" width="78%"></p>

1. Click Click Click Click Click **Açık bir disk görüntüsü**.
2. Keep Keep Keep Keep **Otomatik algılama** etkinleştirin veya makineyi ve formatı manuel olarak seçin.
3. Use Use Use Use Use **Link zoom** Her iki tarafını aynı zoom seviyesinde tutmak.
4. Use Use Use Use Use **reset** İlk görüşü geri yüklemek için.
5. Open Open Open Open **Inspector** Seçilen bölge hakkında ayrıntılı bilgi için.

Efsane normal flux, kısa ve uzun geçişleri, başlıklar, kodlanmış verileri ayırt eder ve anomalileri tespit eder. Çiğ bir görüntü, bilinen bir dosya sistemine kodlanamayan verileri içerebilir, ancak burada hala incelenebilir.

### Bakışı yorumlamak

Her büyük dairesel panel bir disk tarafını temsil eder. Merkez, tarafını ve mevcut veri durumunu tanımlar; konsantrik pozisyonlar parçalara karşılık gelir. Renkler, efsaneye göre tespit edilen bölgeleri sınıflandırır. Görselleştirici soruları cevaplamak için tasarlanmıştır:

- Görüntü bir tarafta veya her iki tarafta da veri içeriyor mu?
- Beklenilen parçalar mevcut mu?
- Disk boyunca izole veya tekrarlanan anomaliler mi?
- Otomatik algılama bir plausible makine ve format tespit etti mi?

Bir anomali rengi bölgeyi incelemenin bir nedenidir, diskin güvenilmez olduğu kanıt değildir. Kopya koruması, standart olmayan formatlama, zayıf bir kayıt ve hasarlı bir sektör bağlamsal yorumlama gerektiren farklı yapılar üretebilir.

### Önerilen denetim dizisi

Bağlantılı zoom ile başlayın, her iki tarafını da aynı boyutta karşılaştırmayı sağlar. Şüpheli bir bölge seçin, açık **Inspector** Ve onu komşu parçalarla karşılaştırın. Sonuç bir algılama problemi gibi görünüyorsa, otomatik algılamayı devre dışı bırakır ve bilinen bir makine ve format seçin. Testten sonra otomatik algılamaya geri dönün, böylece zorla bir ayar yanlışlıkla başka bir görüntü için kullanılmaz.

## Açıklama: Exploring disk Content

The The The The The The The The **Disk Explorer** sekmesi disk görüntüleri bir dosya hiyerarşisi olarak destekledi.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer sekmesi" width="78%"></p>

1. Mevcut bir görüntü açın veya bir disk okuyun.
2. Keep Keep Keep Keep **Otomatik algılama** Bir makine veya format zorlamanız gerektiğinde etkinleştirin.
3. Sayı bilgilerini gözden geçirin: sistem, koruma, dosya sistemi, kapasite, ücretsiz alan ve öğe say.
4. Sol paneldeki yönetmenlere göz atın.
5. Doğru paneldeki ayrıntıları görüntülemek için bir öğe seçin.

Görüntü formatı veya dosya sistemi desteklenmezse, kullanım kullanımı **Görselleştirme** Bunun yerine çiğ yapısını incelemek.

### Panelleri anlamak

Üst özet, monte edilmiş görüntüyü ve tespit edilen hacmi açıklar. Daha düşük sol panel dizin hiyerarşisini içeriyor. Merkezi masa, seçilen dizideki öğeleri isim, değişiklik tarihi, tipi ve büyüklüğü ile listeler. Doğru panel, seçilen öğe için ayrıntıları gösterir.

Disk Explorer Her ham pistin mükemmel bir şekilde kodlandığını ima etmez. Basınç özetini ve öğeyi hızlı bir plaubilite kontrolü olarak kullanın, sonra açık temsilci dosyaları açın veya onları doğruluk önemli olduğunda bilinen bir dizi listeyle karşılaştırın.

### Hiçbir şey göründüğünde

İlk olarak görüntü yolunun doğru olduğunu doğrulayın. Sonra tespit edilen makineyi ve biçimini kontrol edin. Geçerli bir görüntü desteklenmeyen veya hasarlı bir dosya sistemi içerebilir, bu durumda, Explorer boş kalabilir, ancak boş kalabilir. **Görselleştirme** kaydedilen verileri gösterir. Sadece boş bir kaşife dayalı kaynak imajını yazma veya dağıtma.

## araçları kullanarak

The The The The The The The The **Araçlar** sekme grupları Greaseweazle bakım işlemleri.

<p align="center"><img src="images/main-tools-en.png" alt="Araçlar sekmesi" width="78%"></p>

Soldaki listeden bir komut seçin, parametrelerini gözden geçirin, sonra tıklayın **Execute** İmkansız veya donanım değiştirici komutları sadece seçilmiş kontrol ve sürücüyü doğrulamadan sonra kullanılmalıdır.

Çoğu araç dialogları üç alanı içerir: Üstteki parametreler, merkezdeki bir durum ve ham- the alanı ve altta üretilen komut. komut önbellek değişiklikleri seçenekleri etkinleştirilir. Kontrolsüz bir parametre normalde “Bu değeri değiştirmez” anlamına gelir, ancak kontrol edilen bir parametre bu değeri komutta içerir.

Bireysel tanı diyalogları tarif edilir [Donanım tanı ve bakım](#hardware-diagnostics-and-maintenance).

## Emulation

### Kurtarılmış bir makine açın

The The The The The The The The **Emulation ** sekme listeleri kaydedilen konfigürasyonlar. Bir tane seçin ve tıklayın ** Open Open Open Open**Her çalışan makine kendi sekmesinde görünür.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulation welcome screen" width="78%"></p>

Create and edit makineleri in **Seçenekler > Emulation > Yapılar ** ve ** Seçenekler > Emulation > Amiga**.

Bir yapılandırma ortaya çıkarsa, önce Seçeneklerde bir tane oluşturun. Kurtarılan bir yapılandırma, makine modelini, emülatör versiyonunu birleştirir, ROM, hafıza, video, ses, depolama ve giriş haritaları. Bir yapılandırmayı kurtarmak başlamaz; ana geri dön **Emulation ** sekme ve tıklayın ** Open Open Open Open**.

### Run-makine kontrolleri

<p align="center"><img src="images/main-emulation-running-en.png" alt="Koşu emated makinesi" width="78%"></p>

Koşu makineli araç çubuğu, güç, duraklama, sıfır, kurtarma-devlet, yük-devlet, yakalama ve görüntü kontrolleri sağlar. Ayrıca gösteriyor:

- İnşaatlı hızlı ve hızlı yük kısayolları;
- Aktif former, örneğin Direct3D 11;
- Tam ekran ve fare sürüm kısayolları;
- Ses, kontrolör ve fare devleti;
- Mevcut karar, yenileme oranı ve çerçeve oranı.

Emulation ekranının alt kısmındaki disk şerit, her emated sürücü için kullanılabilir medyayı yönetir. Klavye atamaları içinde değiştirilebilir **Seçenekler > Emulation > Kısayollar** Ancak, emated klavye, fare ve kontrol haritaları ilgili olarak yapılandırılır. Amiga sekmeler.

### Toolbar referans

| Kontrol grubu | Amaç |
|---|---|
| Güç ve duraklama | Başlangıçlar, duraklar, duraklar veya emated makineyi özgeçmişler |
| Sıfır kontrol kontrolü | yapılandırılmış yumuşak veya sert sıfırlama eylemi gerçekleştirin |
| Devlet kontrolleri | Kaydetler veya hızlı devam için bir emülatör devleti yükler |
| yakalama | Ölmüş görüntünün bir görüntüsünü kurtarın |
| Ekran görüntüsü | Ekran sunumunu değiştirin veya tam ekranlara girin |
| Hızlı devlet hatırlatması | Aktif kurtarma / yük kısayolları göster |
| Renderer | Raporlar aktif video geriend |
| Giriş hatırlatıcı | Tüm ekranları ve fare sürüm kısayollarını göster |
| Cihaz göstergeleri | Raporlar ses, kontrolör ve fare devleti |
| Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans Performans | Raporlar çıktı büyüklüğü, frekans ve çerçeve oranı |

### Tam ekrandan çıkmak veya fareyi serbest bırakmak

Araç çubuğu şu anda verilen anahtarları gösterir. Örneklenmiş konfigürasyonda, **Alt+ Return Return Return Return Return ** Tam ekran ve ** F12** fareyi serbest bırakır. Görüntülenen değerleri yazar olarak tedavi edin çünkü kısayollar yeniden adlandırılabilir.

### Dresspy media

Sürücü şerit, her emilen sürücüyü tanımlar, örneğin `DF0:`. Medya kontrollerini eklemek, değiştirmek veya bir görüntüyü azaltmak için kullanın. Medyanın değiştirilmesi yalnızca çalışan makinenin yerleştirilmesi diski değiştirir; bu eylem açıkça kurtarılamadığı sürece depolama alanı tanımı değişmez.

## Uygulama seçenekleri

Open Open Open Open **Seçenekleri** Uygulamayı yapılandırmak için ana pencereden.

### General General General General General General General General General General General General

<p align="center"><img src="images/options-general-en.png" alt="Genel seçenekler" width="72%"></p>

The The The The The The The The **General General General General General General General General General General General General** sekme içerir:

- Varsayılan disk görüntü klasörü;
- arayüz dili ve tema;
- dönüşümler için dosya adı-tag nesli;
- Önceden tanımlanmış ve son özel etiket modelleri;
- Canlı bir dosya adı.

Tag değişkenleri kaynak adı, aile, format, uzatma, tarih ve zaman içerir. Varsayılan kalıbı geri yüklemek için reset düğmesine kullanın.

Dosya adı herhangi bir dosya oluşturulmadan önce güncellemeleri gösterir. Tekrarlanan ayırıcıları, eksik uzantıları veya belirsiz isimleri tespit etmek için kullanın. Son özel desenler, mevcut preset değiştirmeden önceki adlandırma şemalarına hızlı erişim sağlar.

### Logs Logs

<p align="center"><img src="images/options-logs-en.png" alt="Log seçenekleri" width="72%"></p>

Logging her operasyon için bağımsız olarak yapılandırılabilir. Her kategori için, logları kurtarmak, maksimum bir dosya boyutunu belirlemek ve önceki logların korunması gerektiğine karar verin. Bir büyüklüğü `0` sınırsız anlamına gelir. **Açık klasör** Mevcut log dizisini açın.

Enable **Önceki logları tutun** Koruma ve teşhis işleri için çeşitli girişimlerin tarihi önemli. Sadece en son sonucun yararlı olduğu zaman onu ayırt edemez. Maksimum boyut sınırları, disk görüntülerini yakalamamak için log depolamaya uygulanır.

### kontrolörler ve sürücüler

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="kontrolörler ve sürücüler" width="72%"></p>

Bu sekmeyi kullanın:

- Bağımlı kontrolörler için tarama;
- Ekle ve sürücü yapılandırmalarını kaldırır;
- Sürücü boyutunu, yoğunluğu ve hızı seçin;
- Donanım ayarları kurtarmak;
- seçmek veya otomatik olarak bulmak `gw.exe`;
- check for and download Greaseweazle Host Tools Güncellemeler;
- Daha önceden yapılandırılabilir bir yol restore edin.

Kaydetilen donanım ayarları, bir sürücü geçici olarak kapandığında kullanılabilir.

#### Bir sürücü ekle

1. Click Click Click Click Click **Scan** ve görünür kontrolörleri bekleyin.
2. Click Click Click Click Click **Bir sürücü ekle** Gerekli sürücü zaten listelenmemişse.
3. Mantıklı sürücü numarasını, fiziksel boyutunu, kayıt yoğunluğunu ve rotasyon hızını seçin.
4. Sırayı kurtar.
5. Gösterdiğini Onaylayın **Mevcut kullanılabilir ** ve ** Configured**.

Çöp kontrolünü sadece kaydedilen yapılandırmayı kaldırmak için kullanın; donanımı bozmaz. Aynı kontrolör farklı görünüyorsa COM port daha sonra, depolama limanının hala geçerli olduğunu varsaymadan önce tekrar tarama yapın.

#### Yönetim Yönetimi Yönetimi Yönetimi Yönetimi Yönetimi Greaseweazle Host Tools

**Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find gw.exe ** Bilinen yerleri araştırın. ** seçin ** Belirli bir executable seçin. ** güncelleştirme için kontrol ** Oluşturulan birini değiştirmeden mevcut versiyonları sorgular. ** Download latest version ** Seçilen mevcut paketi yükleyin ve ** Önceki yolu kullanın ** Daha önceki yapılandırılmış yeri geri yükleyin. Eskileri değiştirdikten sonra, koşmak ** Controller bilgi** Seçilmiş versiyonun kontrolörle iletişim kurabileceğini doğrulamak.

### Motorlar

<p align="center"><img src="images/options-engines-en.png" alt="Motor seçimi" width="72%"></p>

Motoru okuma, yazma, dönüştürme ve yazma için bağımsız olarak seçin Disk Explorer. Seçilmiş motor kesinlikle kullanılır: talep edilen operasyonu yapamıyorsa, GW GUI Sessiz geçiş motorları yerine sınırlamayı rapor edin.

Bu bağımsızlık kasıtlıdır. Örneğin, fiziksel okumalar kullanabilir Greaseweazle Host Tools Görüntü dönüşümü ve keşif iç motoru kullanırken. Bir profilde veya proje notunda yedeklenebilirlik önemlidir.

### Profiller

<p align="center"><img src="images/options-profiles-en.png" alt="Profiller" width="72%"></p>

Profiller okumak, yazmak ve dönüşüm işlemleri için yeniden kullanılabilir ayarlar. İlgili kategoriyi profillerini yönetmek için seçin. Seçilen bir profil ana-window durumunda ve işlem ekranlarında gösterilir.

Uzman bayrakların açıklanmamış koleksiyonlarından ziyade tekrarlanabilir iş akışları için profiller kullanın. Her profili belirli bir sürücü, disk ailesi veya kurtarma yöntemi gibi belirli bir isim verin. Alt motorunu güncellemeden sonra bir profili gözden geçirin çünkü desteklenen seçenekler değişebilir.

## Emulation seçenekleri

The The The The The The The The **Emulation** Seçenekler genel depolama ayarlarını, küresel kısayolları, kaydedilen konfigürasyonları ve makineye özgü ayarlar içerir.

### Genel emulation klasörleri

<p align="center"><img src="images/options-emulation-general-en.png" alt="Genel emulation seçenekleri" width="72%"></p>

Paylaşılan emulation depolama klasörü ve yakalamak ve kurtarmak için varsayılan klasörleri ayarlayın. **Açık klasör** File Explorer'daki paylaşılan yeri açar.

Limitleri tut ve ülkeleri ayrı klasörlerde kurtarın. Bir yakalama sıradan bir görüntüdür; kurtarılan bir devlet, emülatöre özel makine devleti içeriyor ve onu yaratan emülatör sürüme ve konfigürasyona bağlı olabilir. Önemli kurtarılan devletlerle birlikte yapılandırma ve medya geri dön.

### Global kısayollar

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Emulation kısayolları" width="72%"></p>

Bir eylem veya anahtar atama için arayın veya kısayolları, geri yükleme varsayılanları ve açık çatışmaları ortadan kaldırır. Durum sütunu geçerli ve çatışma atamalarını tanımlar.

Bir kısayol değiştirmek için, eylemi bulmak, tıklayın **Assign **, ve istenen anahtar kombinasyonu basın. Seçenekleri kapatmadan önce durumu kontrol edin. ** Gümrük çatışmaları ** Çatışma atamalarını ortadan kaldırır; varsayılan haritayı geri yüklemez. Use Use Use Use Use ** Geri yükleme varsayılanleri geri yükleme** Standart setle özel atamaları değiştirmek istediğinizde.

### Kurtarılan yapılandırmalar

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Saved Emulations" width="72%"></p>

Bu sayfa listeler makineleri kurtardı. Bunu düzenlemek için bir yapılandırma seçin **Amiga** sekme. Listeyi yenileyebilir veya seçilmiş yapılandırmayı silebilirsiniz.

Bir yapılandırmayı bitiren makine tanımını ortadan kaldırır. Bu, eject medyası için veya çalışan bir makineyi kapatmanın bir yolu olarak kullanılmamalıdır. Deletion önce, not any ROMAncak sabit görüntü ve yapılandırma ile ilişkili devlet dosyaları.

## Amiga yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma yapılandırma

Mevcut arayüz ayrıntılı sağlar Amiga konfigürasyon sayfaları. Aynı ayarlar yapısı, ana iş akışını değiştirmeden başka emilen sistemler için uzatılabilir.

### General General General General General General General General General General General General

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga Genel ayarlar" width="72%"></p>

seçin the Select the Amiga Model, yapılandırmayı kurtarın veya emülatör versiyonunu değiştirin ve zor diskler ve diğer medya için varsayılan klasörler tanımlayın. **Arama versiyonları** Resmi telif hakkı kaynağı sorgular.

Modeli ile başlayın çünkü daha sonraki sayfaları kısıtlar. Değiştirin, mevcut durumu değiştirebilir CPUbellek, ROM, çipet ve depolama seçenekleri. Bir emülatör versiyonunu seçtikten sonra, ana pencereden başlatmadan önce yapılandırmayı tasarruf edin. Başka bir emülatör sürümünü yüklemek bu yapılandırma tarafından kullanılan sürümü değiştirir; makinenin ikinci bir kopyasını yaratmaz.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU ayarlar" width="72%"></p>

The The The The The The The The CPU Sayfa, makine modeli tarafından seçilen işlemciyi gösterir ve uyumlu hassas sağlar, FPU, ve hız seçenekleri. Seçilen modele uygulanmayan seçenekler devre dışı kalır.

- **CPU model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model model** Emated işlemciyi tanımlayın.
- **Hassasiyet** zamanlama modelini kontrol edin. Çevrim-exact modları iyi donanım uyumluluk ancak daha host işleme gerektirir.
- **FPU** Desteklenen zaman uyumlu bir yüzen nokta birimi sağlar.
- **CPU Hız hızı hız hızı** Orijinal zamanlama veya hızlandırılmış bir mod seçin.

Bir temel yapılandırma için, model-derived CPU ve orijinal hız. Sadece makine standart ayarlarında doğru bir şekilde hızlanıyor.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM ayarlar" width="72%"></p>

Configure Chip RAMSlow, RAMHızlı, RAMVe genişleme hafızasını destekledi. Uyumluluk mesajları seçilmiş makine için kısıtlamalar açıklar ve toplam yapılandırılmış hafıza altta gösterilir.

**Chip Chip RAM ** Özel çiplere erişilebilir ve platform tarafından gereklidir. ** Yavaş yavaş yavaş yavaş yavaş yavaş RAM ** Ortak konfigürasyonlar tarafından kullanılan uyumlu genişleme hafızasını temsil eder. ** Hızlı Hızlı Hızlı Hızlı RAM ** işlemci odaklı genişleme hafızadır. ** Zorro III RAM** Sadece genişleme mimarisini destekleyen modeller için geçerlidir. Uyumluluk mesajları ve engelli kontroller, seçilen modelin temsil edemeyeceği kombinasyonları önler.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM ayarlar" width="72%"></p>

Sistemi seçin Kickstart ROM, Seçmeli genişletilmiş genişletilmiş ROMVe ROM anahtar. tespit edilen -ROM Liste isimleri, revizyonları ve seçilen modelle uyumluluk gösterir. Bir tespit edilen ROM ve tıklayın **Use Use Use Use Use**, veya bir dosyaya manuel olarak göz atın.

ROM dosyalar tarafından temin edilmez GW GUIROM'ları kullanmak yasal olarak izin verilir.

Tespit edilen liste bir dosya adından tahmin etmek için tercih edilir: rapor eder ROM Kimlik ve revizyon ve seçilen modelle uyumluluğu değerlendirin. **Uyumlu Uyumlu Uyumlu ** Normal seçimdir; ** Kısmen uyumlu ** Belirtildiğine göre, ROM boot olabilir ama tam olarak makineyle eşleşmez. ** Yenileme ** Yeniden yapılandırılabilir ROM yerler. ** Use Use Use Use Use** Seçilen tespit edilen atamaları düzenler ROM yapılandırmaya.

### Video Video Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga Video ayarları" width="72%"></p>

Video standardını yapılandırın, yön oranı, karar, çizgi modu, sınır ekleme, turner, renkli derinlik, çerçeve atlama, gamma ve flicker düzeltme. Ek cips ayarları, seçilen model tarafından desteklenen sayfayı daha da aşağıda bulunmaktadır.

| ayar ayarı | Pratik etki |
|---|---|
| Video standart | Selects PAL veya NTSC zamanlama ve beklenen yeni davranışın |
| Aspect oranı | Emated resmin nasıl ölçekleneceğini kontroller |
| Karar | Otomatik veya açık çıkış detayını seçin |
| Line modu | Interlaced veya line-doubled çıktı kontrolü |
| Ekin sınırları | Sadece etkinleştirilen Overscan'ı çıkartın |
| Rendering | Grafikleri tekrar seçin |
| Renk derinliği | Çıktı renkli hassas |
| Frame at | etkinleştirilen çerçeveleri azaltır |
| Gamma Gamma | Optimizasyon cevabı |
| Flicker Fixer | Aksi takdirde visibly flicker |

Bir görüntüyü bir seferde değiştirin. Emülasyon penceresi boş veya kararsız hale gelirse, otomatik karara geri döner, engelli çerçeve atılır, tarafsız kumar ve daha önce çalışan.

### Audio Audio Audio

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga Ses ayarları" width="72%"></p>

Enable veya devre dışı ses, çıktı cihazı ve gecikmeyi seçin, sonra interpolasyon yapılandırın, Amiga filtreleme, filtre türü, stereo ayrılık, floppy-drive ses ve CD-audio hacmi.

Alt latency gecikmeyi azaltır, ancak yoğun bir bilgisayarda düşüşe neden olabilir. Eğer ses çatlakları artarsa onu artırın. Interpolasyon ve Amiga Ses filtre değişikliği, emated program mantığından ziyade ses yeniden üretilir. Drive-sound hacmi, normal mekanik sesi normalden ayrı olarak kontrol eder Amiga Ses.

### Depolama

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga depolama ayarları" width="72%"></p>

Depolama sayfası cihazı tanımlayıcıları, türleri, modeller, ilişkili medyaları ve mevcut eylemleri listeler. Ekle, yapılandırın veya buradaki cihazları kaldırın. Floppy diskler ve CDler doğrudan çalışan bir makineden alınabilir veya değiştirebilir.

The The The The The The The The **aygıt tanımlayıcısı ** Emated sistem cihazı nasıl ele alır. ** Tipi Tipi Tipi Tipi ** Diskpy, sert-disk, optik ve diğer desteklenen cihazlar ayırt eder. ** Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model Model ** Emated donanımı açıklarken, ** Associated media** Şu anda verilen görüntüyü tanımlayın. Cihazı değerli değerli tarif edilebilir medyadan önce yapılandırın ve sert görüntülerin yedeklerini tut.

### Klavye

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga klavye ayarları" width="72%"></p>

Arama Arama Amiga Anahtarlar ve ev sahibi atamalar, yeni anahtarlar atamalar, geri yükleme varsayılanleri veya açık çatışmaları. Durum sütunu her atamanın geçerli olup olmadığını bildiriyor.

Sol sütunu emated Amiga Anahtar; **Association Association Association** Ev sahibi anahtar kombinasyonu gösterir. Geçerli bir haritalama, Windows veya uygulama aynı kısayolu rezerve ederse hala uygunsuz olabilir, bu nedenle çalışan makine içinde kritik kombinasyonlar test edebilir. Gizli yazılımların sık sık ihtiyaç duyduğu bir anahtara fare sürüm veya tam ekran kısayolunu atamaktan kaçının.

### Mouse

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga fare ayarları" width="72%"></p>

Fiziksel fare hızını ayarlayın, hangi analog çubuğun fareyi kontrol ettiğini seçin, analog ölü bölgeyi ve hızını ayarlar ve fare-action haritalarını yapılandırın. Gerekli olduğunda varsayılan veya net haritalama çatışmalarını geri yükleyin.

Bir kontrolör noktası sürüklerse ölü bölgeyi artırın. Her iki çubuğun etkinleştirildiği zaman sol ve sağ el hızı. Daha düşük haritalama masası, fare düğmeleri veya eylemlerle ev sahipliği yapan girdileri ilişkilendirir; başka yerlerde denetimli haritalamalarından sonra çatışma durumunu kontrol edin.

### Controllers

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga kontrolör ayarları" width="72%"></p>

Bağlantılı kontrolörler, cihazları ve kontrol türlerini tespit etmek için tayin eder Amiga limanlar ve yapılandırma kontrol haritaları ve turbo-fire ayarları. Mevcut seçimler tespit edilen donanıma ve seçilmiş makineye bağlıdır.

Port 1 ve Port 2 bağımsız olarak yapılandırılır. **Otomatik Otomatik Otomatik Otomatik Otomatik** Kontrol türü mantıklı bir başlangıç noktasıdır, ancak belirli bir sevinç veya fare bekleyen yazılım açık bir tür gerektirebilir. Yeni bir bağlantılı bir denetleyici atamadan önce tespit edin. Turbo yangın defalarca haritalanmış bir girişi etkinleştirir ve oyun veya uygulama faydalarından yararlanmaksızın devre dışı kalmalıdır.

## Donanım tanı ve bakım

Bu diyaloglar açılır **Araçlar ** sekme. Her dialog, üretilenleri gösterir Greaseweazle komut. İncelemeden önce ** Execute**.

### Controller bilgi

<p align="center"><img src="images/tool-controller-information-en.png" alt="Controller bilgi" width="62%"></p>

Seçilen kontrolör tarafından bildirilen görüntüler. Genişletilmiş Genişleme **Raw çıktı** Tam komut cevabına ihtiyacınız olduğunda.

Bunu ilk tanı emri olarak kullanın. Başarılı bir cevap, doğruları doğruluyor GW GUI yapılandırılmış Host Toolsları kapatılabilir ve seçilen cihazla iletişim kurabilir. Bir güncelleme yapmadan önce bilgisayar ve donanım bilgilerini kayıt edin.

### USB bant genişliği

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB bant genişliği" width="62%"></p>

Mevcut ölçümler USB İletişim bant genişliği. istikrarsız transferleri veya uygun olmayan bir şekilde teşhis etmek için kullanın USB bağlantı.

Testten önce kontrolör kullanarak diğer yazılımları kapatın. Ölçümü değiştirmekten sonra tekrarlayın USB port, kablo veya merkez. Tek bir ölçümü mutlak bir garanti olarak tedavi etmek yerine benzer koşullar altında karşılaştırma sonuçları.

### Drive hız

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Drive hız" width="62%"></p>

Sürücü rotasyon hızını ölçer. Daha fazla temsilci sonuca ihtiyacınız olduğunda ölçüm sayısını artırın.

Tek bir ölçüm hızlı bir kontroldür; birkaç ölçüm hızın stabil olup olmadığını ortaya çıkarır. Sürücü sonucu yorumlamadan önce normal hıza ulaşalım. Beklenmeyen bir değer yanlış yapılandırılmış bir hız, mekanik bir konu veya ölçüm oluşturma problemi gösterebilir.

### Seek kafa

<p align="center"><img src="images/tool-seek-head-en.png" alt="Seek kafa" width="62%"></p>

Sürücü kafasını seçilmiş bir silindire taşır. **Aşırı silindirlere izin verin ** Normalde sınırlı pozisyonları sınırlandırır ve ** Motor aktif olarak tutun** Operasyon sırasında çalışan motordan ayrılır. Yalnızca donanım prosedürü açıkça onları gerektirdiğinde aşırı pozisyonları kullanın.

Normal arama, bir tanıdan önce baş hareketini veya konumlandırmayı doğrulamak için faydalıdır. İstenen silindirin sürücü için uygunsuz olup olmadığını anormal tekrarlanan etkiler için dinleyin. Bu araç hedef silindirde verileri okumaz veya doğrulamaz.

### Ağlama tanı teşhis

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Ağlama tanı teşhis" width="62%"></p>

Runs tekrar sürüş-ment analizi için okur. İzleme seçimi, devrim ve sayıları, kodlama formatı, ham flux, indeks, hız, PLL, yoğunluk-pin, sert-sector, TG43, ve ters-data seçenekleri. Uygun referans medya ve donanım bilgisi gerektirir.

Bilinen bir referans diski ve en küçük overrides seti ile başlayın. **Yaşlama parçaları ** parçaları tanımlar ve örneklenir; ** Devrimler per track ** Her örnek süresi kontrol eder; ** Okunma Sayısı** Tekrarları belirler. Özel bir disk tanımı veya kodlama formatı sadece referans medyayı oynarken. Sahte indeks, sert sektörler gibi seçenekler, PLL Overrides, yoğunluk pins ve TG43 Donanım veya formata özgüdür ve yanlış kullanıldığında bir karşılaştırmayı geçersiz kılar.

### Donanım pimleri

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Donanım pimleri" width="62%"></p>

Destekleyici bir kontrol pimini okuyun veya değiştirir. pin seçin, etkinleştirin **Değişim pin ** Sadece bir değer yazarken ve seçin ** Yüksek seviye** Amaçlı donanım işlemi gerektiğinde.

With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With With **Değişim pin** Engelliler, komut pimi sorgular. Bu daha güvenli varsayılandır. Bir seviyeyi doğrudan kontrol I/O'yu etkiler ve sadece doğru ile yapılmalıdır. Greaseweazle Donanım belgeleri ve ek sürücü kabloları.

### Sıfır kontrolör

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Sıfır kontrolör" width="62%"></p>

Tamamlayın Greaseweazle kontrolör. Kontrolör tespit edildiğinde bunu kullanın, ancak artık normal olarak yanıt vermez.

Yeniden tanımlamadan önce bitirmek için herhangi bir aktif disk operasyonu bekleyin. Daha sonra, kontrol durumunu otomatik olarak kurtarmazsa tekrar tarama. Bir reset yanlışı tamir etmiyor `gw.exe` Yol veya bir kapanış USB Cihaz.

### Gecikmeler

<p align="center"><img src="images/tool-delays-en.png" alt="Controller gecikme gecikme gecikmeleri" width="62%"></p>

Seçme, baş adım, yerleşme, motor, otomatik deseleksiyon, zamanlaması ve indeks maske gecikmeleri dahil olmak üzere kontrolör zamanlama değerlerini okuyun. Enable sadece değiştirme niyetinde olduğunuz değerler.

Kontrolsüz alanlar, ilgili kontrolör değerini değişmeden bırakır. Düzenlemeden önce, mevcut değerleri kaydetmek. Timing değişiklikleri her bir sonraki fiziksel operasyonu etkileyebilir, bu nedenle uygulanabilir medya ile test edebilir ve davranışın güvenilmez hale gelmesi durumunda bilinen iyi değerleri geri yükleyebilir.

### Şirketware

<p align="center"><img src="images/tool-firmware-en.png" alt="Firmware update update update" width="62%"></p>

Updates kontrolörü. **Update bootloader** Açıkça riskli olarak işaretlenir ve resmi bilgisayar prosedürü gerektirdiği sürece devre dışı kalmalıdır. Bir güncelleme sırasında kontrolörü kapatmayın.

Güncellemeden önce, bağlantılı kontrolörü onaylayın **Controller bilgi**, istikrarlı bir doğrudan kullanın USB bağlantı ve ona erişebilecek diğer yazılımları kapatın. Tamamlandıktan sonra, kontrol cihazını yeniden bağlayın ve raporlanan bilgisayar versiyonunu doğrulamak için bilgilerini tekrar okuyun.

## Logs and operation history

Operasyon tarihini operasyon tarafından kaydedilen logları incelemek için açın.

<p align="center"><img src="images/operation-history-en.png" alt="Tarih" width="68%"></p>

İçeriğini görüntülemek için solda bir log seçin. **İhracat İhracatı** Tanık veya destek için bir kopyasını kurtarır. Yollar ve komut hatları kişisel klasör isimleri içerebilir, bu yüzden onları paylaşmadan önce ihraç edilen loglar.

Ana pencerede canlı konsol mevcut komut ve son çıktı gösterir. Onun kopya düğmesi görüntülenen metni kopyalar.

### Bir günlük okuma

Yararlı bir teşhis kaydı, üretilen komut, zamantamps, motor çıktısını ve son durumu içerir. Alt üstten çalışın: son hatayı tespit edin, sonra ilk uyarıyı bulun veya daha önce onu takip etmedi. Daha sonra genel bir başarısızlık genellikle daha önceki, daha spesifik bir mesajın sonucudur.

İki denemeyi karşılaştırırken, kontrolörü, sürücü, motor, profil, kaynak yolu, çıktı formatı ve uzman argümanların aynı olduğunu kontrol edin. Aksi takdirde, farklı bir sonuç disk istikrarsızlıktan ziyade değişen ayarları yansıtabilir.

## Uygulama verileri ve portatif kullanım

GW GUI Kullanıcı verileri başvuru binerlerinden ayrı tutar. Seçilen paket ve moda bağlı olarak, ayarlar, loglar, indirilen araçlar, emülatör bileşenleri, yakalamalar, devletler ve makine yapılandırmaları uygulamada da depolanır. `Data` Rehber veya yapılandırılmış kullanıcı konumlarında.

Bir portatif yükleme değiştirme veya taşımadan önce, tüm uygulama klasörü birlikte tut ve geri dön `Data` klasörü. Bireysel dosyaları hareket etmeyin `lib`Çünkü uygulama kendi ve üçüncü taraf kütüphanelerini bu yapıdan çözer.

### Önerilen yedekleme içeriği

İş akışınız için önemli olduğunda aşağıdakilere geri dönün:

- Uygulama ayarları ve profilleri;
- kontrolör ve sürücü tanımları;
- Bağışlama konfigürasyonları;
- ROM yollar ve yasal olarak düzenlenmiş ROM yedeklemeler;
- Sert-disk ve çıkarılabilir-media görüntüleri;
- Yakalar ve kurtarılan devletler;
- Koruma kayıtları olarak kullanılan operasyon logları.

Disk görüntüleri ayarlardan çok daha büyük olabilir. Mağaza Archival ustaları sadece mümkün olduğunda ve kopyalarda çalışır.

## Önerilen akışlar

### Bilinmeyen bir disk

1. Inspect ve uygun bir bakım prosedürü kullanarak sürüşü temizleyin.
2. Mümkün olursa diski yazın.
3. Select Select Select **Read > Raw image (İngilizce).SCP)**.
4. Descriptive filename kullanın ve normal pist aralığını birden fazla devrimle okuyun.
5. Konsolu gözden geçirin ve logları kurtarın.
6. Inspect her iki tarafta da **Görselleştirme**.
7. Büyük sektör formatlarına bir kopyasını dönüştürül.
8. dönüştürülmüş kopyaları test edin **Disk Explorer** veya uygun yazılım.
9. Çiğ efendiyi, logunu ve birlikte notları koruyun.

### Bir görüntüden bir diski görüntüleyin

1. Görüntüyü araştırın ve beklenen ailesini ve biçimini doğrulayın.
2. Doğru büyüklükteki ve yoğunluğun geniş veya kasıtlı olarak uygulanabilir bir diski açın.
3. Open Open Open Open **Write Write Write Write** Ve görüntüyü seçin.
4. yapılandırılmış sürücüyü ve tespit edilen formatı doğrulayın.
5. Diski yazın.
6. Bunu ayrı bir doğrulama görüntüsüne geri okuyun.
7. Decoded içeriği karşılaştırın ve şüpheli parçaları görsel olarak gözden geçirin.

### Bir emated yaratmak Amiga

1. Open Open Open Open **Seçenekler > Emulation > Yapılar** ve bir makine oluşturun veya seçin.
2. İçinde In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In In **Amiga > General**, modeli ve emülatör versiyonunu seçin.
3. Bir uyumlu olarak, yasal olarak elde edilen ROM.
4. Model varsayılanlarını tutmak için CPU ve RAM İlk çizmede.
5. Muhafazakar otomatik ayarlarla video ve ses yapılandırın.
6. Depolama cihazları ekleyin ve kopyalanan medya görüntülerini ekleyin.
7. Analiz klavye, fare ve kontrolör atamaları.
8. Yapıyı kurtar.
9. Geri dön **Emulation **, onu seçin ve tıklayın ** Open Open Open Open**.
10. Sadece başarılı bir önyüklemeden sonra, bir seferde bir hız veya gelişmiş ayarlar.

## Güvenlik kontrol listesi

Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce **Read Oku**:

- Kaynak diski doğru sürücüdedir;
- Kaynağın mümkün olduğu yerde korunması;
- Çıktı yolu mevcut bir usta yazmayacak;
- Profil ve pist diski diskle eşleştirir.

Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce Önce **Write Write Write Write ** veya ** Erase**:

- Hedef disk yok edilebilir;
- Görüntü ve sürücü doğru;
- Disk boyutu ve yoğunluk uyumlu;
- Hiçbir arşiv usta hedef olarak kullanılıyor.

Donanım değişen bir araçtan önce:

- Başka bir operasyon çalışmıyor;
- Doğru kontrolör seçilir;
- Mevcut değerler kaydedildi;
- Kontrol stabil güce sahiptir ve USB Bağlantı;
- Eylem donanım belgeleri tarafından desteklenir.

## Sorun Giderme

### Kontrol listelenmez

1. Kontrolü doğrudan bilgisayara bağlayın.
2. Open Open Open Open **Seçenekler > kontrolörler ve sürücüler**.
3. Click Click Click Click Click **Scan**.
4. Kontrol durumunu onaylayın ve konfigürasyonu kontrol edin.
5. Run Run Run **Controller bilgi** Eğer tespit başarılı olursa ancak komutlar başarısız olur.

Hala görünmüyorsa, başka bir doğrudan deneyin USB port ve kablo, sonra yeniden kullanılabilir. Yeni tespit edilmiş bir seri cihazı için Windows Device Manager'ı kontrol edin. Windows'a görünür bir kontrol cihazı, ancak mevcut değil GW GUI Genellikle yoğun bir limana, sabit yapılandırmaya veya Host Tools problemine işaret eder; Windows puanlarından olmayan bir kontrol USBgüç, sürücü veya donanım.

### `gw.exe` bulunamadı

Open Open Open Open **Seçenekler > kontrolörler ve sürücüler ** Sonra kullanın **Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find Find gw.exe **, ** seçin **Ya da ** Download latest version**. Belirlenen yol puanlarının amaçlanana işaret ettiğini onaylayın Greaseweazle kurulum.

Onu seçtikten sonra, koşmak **Controller bilgi**. Donanımla iletişim kurmadan önce başarısız olursa, geçersiz bir yol için giriş yapın, eksik dosyalar veya başlatamayan bir sürüm.

### Bir operasyon yanlış motoru kullanır

Open Open Open Open **Seçenekler > Motorlar** Ve motoru bu tam operasyona atanan kontrol edin. GW GUI Sessizce diğer motora geri düşmez.

Motor ayarları ayrıdır: dönüşüm motorunu değiştirmek okuma, yazma veya yazmaz veya Disk Explorerseçeneği tasarruf ettikten sonra başarısız işlemi yeniden açın ve konsolda üretilen komutu onaylayın.

### Bir görüntü tanınmamıştır

Uygun otomatik algılama sadece doğru makineyi ve biçimini biliyorsanız. Aksi takdirde, deneyin **Görselleştirme** Görüntüyü daha düşük bir seviyede incelemek için sekme.

Kaynağın bir ham flux yakalama olup olmadığını kontrol edin, bir sektör imajı, sıkıştırılmış bir konteyner veya yanıltıcı bir uzatma ile ilgili olmayan bir dosya. Hiçbir zaman sadece algılamayı zorlamak için bir uzatma yeniden adlandırma; dönüşüm kaynak yapısını doğru şekilde yorumlamalıdır.

### Emulation başlamaz

Kurtarılan yapılandırmayı doğrulayın, oluşturulan emülatör sürümünü seçin, seçilmiş ROM, depolama yolları ve model uyumluluğu. Uygulama logunu tam hata detayları için gözden geçirin.

Temporly geri dönüş CPU, RAM, video ve basit bir model uyumlu bir temele depolama. Eğer taban başlarsa, bir seferde bir özel ayar geri yükleyin. Temiz bir önyükleme çalışması sırasında başka bir emülatör versiyonu veya makine tanımı ile yaratılan bir devlet de başarısız olabilir.

### Bir kısayol veya giriş çalışmıyor

Her ikisini de küresel kontrol edin **Emulation > Kısayollar** Sayfa ve makineye özgü klavye, fare veya kontrol sayfası. Çatışma olarak işaretlenen herhangi bir görevi geri alın.

Eğer fare yakalanırsa, çalışan makineli araç çubuğunda gösterilen sürüm kısayolunu kullanın. Seçenekler açıldıktan sonra kontrol cihazı bağlantılı olsaydı, onu atamadan önce tekrar kontrol cihazı tespit edildi.

### Bir komut beklenmedik bir şekilde başarısız olur

1. Canlı konsol çıktısını okuyun.
2. Open Open Open Open **Tarih** Tamamlanan log için.
3. Seçilmiş kontrol, sürücü, profil, motor ve dosya yollarını onaylayın.
4. Tanı için paylaşılmalıdırsa ilgili oturum açın.

### Ses çatlakları veya duraklar

Emulation audio latency, close CPU- Yoğun uygulamalar ve video çerçevesini geri döndürür ve önceki değerlere hızlanır. Planlanan Windows ses cihazının seçildiğini doğrulayın. Bir kez ayarlayın, böylece etkili düzeltme tanımlanabilir.

### Emülasyon ekranı boş veya yavaş

Return solution and line mod to return solution **Otomatik Otomatik Otomatik Otomatik Otomatik**, geçici olarak ayarlanan çerçeve atlayarak ve flicker'i devre dışı bırakmak ve daha önce çalışan fikre deneyin. Yapılının ROM ve eklemeli boot medyası geçerlidir. The The The The The The The The FPS gösterge, sadece önyüklememiş bir makineden bir ekran performansını ayırt etmenize yardımcı olur.

### Bir okuma dengesiz parçaları içeriyor

Okumayı yeni bir dosya adına tekrarlayın, uygun olan devrimleri arttırır ve etkilenen parçaları karşılaştırır. Sürücü kafalarını doğru bir prosedür kullanarak temizleyin ve diski fiziksel hasar için kontrol edin. defalarca visibly hedding veya hasarlı medya okumayın, çünkü daha fazla geçiş bunu kötüleştirebilir.

## Parlak

| Term Term Term Term | Anlam içinde GW GUI |
|---|---|
| Controller | The The The The The The The The Greaseweazle Donanım arayüzü birbirine bağlı USB |
| Drive Drive Drive | Kontrolücüye bağlı fiziksel floppy sürücü |
| Engine Engine Engine | Uygulama, bir operasyon gerçekleştirmek için seçilmiş |
| Flux | Manyetik geçişleri temsil eden bilgi bir diskten okunur |
| Raw image | Bir yakalama düşük seviyeli disk bilgilerini korur, örneğin SCP |
| Sektör imajı | Mantıklı sektörlere organize edilmiş bir temsil |
| Devrim Devrimi | Bir iz okurken tam bir rotasyon örneği alındı |
| Silindir | Bir radyal kafa pozisyonu; bir silindir her tarafta bir iz içerebilir |
| Head Head | Fiziksel sürücü tarafından seçilen disk tarafı |
| Profil Profili | Bir operasyon için yeniden kullanılabilir bir ayar |
| ROM | Anonim bir makine tarafından gerekli olan şirketware görüntüsü |
| Kurtarılan devlet | Çalışan bir emülatörün makinesi durumu |
| Renderer | Grafikler emulation çıktısını göstermek için kullanılır |

## Hızlı referans

| Eğer istersen... | Git... |
|---|---|
| Fiziksel bir diski koruma | **Read Oku** |
| Bir disk üzerinde bir görüntü koyun | **Write Write Write Write** |
| Başka bir görüntü formatı oluşturmak | **Dönüşüm Dönüşüm Dönüşümü** |
| Inspect parçaları veya flux anomalileri | **Görselleştirme** |
| Bir görüntünün içindeki dosyalar | **Disk Explorer** |
| Kontrolcü iletişim | **Araçlar > Controller bilgi** |
| Önlem yolu rotasyon | **Araçlar > Drive hız** |
| Önceki bir komut | **Tarih** |
| Donanım | **Seçenekler > kontrolörler ve sürücüler** |
| Uygulamaları seçin | **Seçenekler > Motorlar** |
| Oluşturulmuş bir makine oluşturun veya düzenler | **Seçenekler > Emulation** |
| Kurtarılan bir makineye başlayın | **Emulation** |
