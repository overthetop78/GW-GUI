# GW GUI Hướng dẫn người dùng

GW GUI là một ứng dụng của Windows cho việc đọc, viết, chuyển đổi, thanh tra, và mô phỏng hình mềm mềm. Nó có thể kiểm soát Greaseweazle Phần cứng, làm việc với các tập tin hình ảnh đĩa thông qua cơ chế nội bộ của nó, và chạy lưu mô phỏng cấu hình máy tính.

Hướng dẫn này mô tả giao diện tiếng Anh hiển thị trong phiên bản hiện thời của ứng dụng. Nó được viết như là nguồn của sổ tay người dùng có thể in được: ảnh chụp màn hình minh họa điều khiển, trong khi văn bản xung quanh giải thích những gì cần lựa chọn, tại sao nên chọn, và làm thế nào để kiểm tra kết quả.

> **Quan trọng:** Đọc đĩa thì không phá hủy được. Viết, xoá, cập nhật phần mềm và một số công cụ phần cứng có thể thay đổi phương tiện truyền thông hay phần cứng. Đọc lời cảnh báo được gắn vào thủ tục thích hợp trước khi nhấn vào ** Thực hiện**.

### Cách sử dụng hướng dẫn này

Nếu đây là lần đầu tiên bạn sử dụng GW GUI, hoàn tất [Bắt đầu](#getting-started)Vậy thì theo tôi. [Đọc đĩa](#reading-a-disk)Nếu ứng dụng này đã được cấu hình, hãy đi thẳng đến chương trong thao tác này. Các chương tùy chọn được dùng để tham khảo khi một thủ tục yêu cầu bạn thay đổi ổ đĩa, động cơ, hồ sơ, hoặc thiết lập máy tính mô phỏng.

Name **táo** Tên tập tin, đường dẫn, lệnh và giá trị nghĩa đen được hiển thị như `code`Ghi chú giải thích cách ứng xử bình thường; cảnh báo xác định các thao tác có thể thay đổi đĩa, điều khiển hoặc cấu hình đã lưu.

## Nội dung

1. [Hiểu được dòng chảy công việc](#understanding-the-workflow)
2. [Bắt đầu](#getting-started)
3. [Cửa sổ chính](#main-window)
4. [Đọc đĩa](#reading-a-disk)
5. [Viết đĩa](#writing-a-disk)
6. [Đang chuyển đổi ảnh đĩa](#converting-disk-images)
7. [Hiển thị ảnh đĩa](#visualizing-a-disk-image)
8. [Nổ tung nội dung ổ đĩa](#exploring-disk-contents)
9. [Dùng công cụ](#using-the-tools)
10. [Mô phỏng](#emulation)
11. [Tùy chọn ứng dụng](#application-options)
12. [Tùy chọn tạo](#emulation-options)
13. [Amiga cấu hình](#amiga-configuration)
14. [Những chẩn đoán và bảo trì phần cứng](#hardware-diagnostics-and-maintenance)
15. [Bản ghi và lịch sử thao tác](#logs-and-operation-history)
16. [Name](#application-data-and-portable-use)
17. [Comment](#recommended-workflows)
18. [Danh sách an toàn](#safety-checklist)
19. [Name](#troubleshooting)
20. [Bản chú giải](#glossary)
21. [Tham chiếu nhanh](#quick-reference)

## Hiểu được dòng chảy công việc

GW GUI Phân biệt hoạt động vật lý-dik với thao tác ảnh:

| Vào | Nhập | Xuất | Trang đã gợi ý |
|---|---|---|---|
| Bảo tồn đĩa mềm | Đĩa | Tập tin ảnh | **Đọc** |
| Tạo lại đĩa mềm | Tập tin ảnh | Đĩa | **Ghi** |
| Thay đổi định dạng ảnh | Tập tin ảnh | Tập tin ảnh một hay nhiều | **Chuyển đổi** |
| Kiểm tra dấu vết và dị thường | Tập tin ảnh | Phân tích trực quan | **Hình ảnh hoá** |
| Duyệt tập tin được cất trong ảnh | Hệ thống ảnh/ tập tin đã hỗ trợ | Tập tin và thư mục | **Disk Explorer** |
| Chẩn đoán ổ đĩa hay điều khiển | Greaseweazle Phần cứng | Đo hay đo trạng thái | **Công cụ** |
| Chạy một máy ảo đã lưu | Cấu hình máy lưu | Phiên chạy tổ chức | **Mô phỏng** |

Để bảo toàn, trước tiên hãy bắt sống và giữ nó không thay đổi như một bậc thầy. Tạo ra những bản sao đổi mới hoặc sửa chữa từ người chủ đó. Điều này tránh lặp lại một đọc vật lý và bảo tồn thông tin mà định dạng dựa trên phân khu có thể không giữ lại.

## Bắt đầu

### Cần thiết

- Cửa sổ với Microsoft .NET Name
- A Greaseweazle Điều khiển các hoạt động mềm yếu.
- Một đường dẫn đã cấu hình tới `gw.exe` khi sử dụng Greaseweazle Host Tools Động cơ.
- Nhận được hợp pháp ROM Tập tin khi một máy tính mô phỏng cần thiết.

Ứng dụng sẽ kiểm tra giờ chạy yêu cầu khi khởi động. Nếu nó còn thiếu, hãy theo dấu nhắc cài đặt, rồi khởi chạy lại GW GUI.

### Trước khi kết nối phần cứng

Hãy kiểm tra những điều sau đây trước khi chạy thao tác tự động:

1. Kết nối Greaseweazle Điều khiển đến chuồng ngựa USB cảng.
2. Kết nối cáp mềm với hướng đúng.
3. Kết nối nguồn cung cấp năng lượng ổ đĩa trước khi chèn phương tiện có giá trị.
4. Xác nhận kích cỡ và mật độ tương ứng với đĩa.
5. Ghi- bảo vệ đĩa nguồn khi có thể.

GW GUI Không thể ngăn ngừa những thiệt hại do sự tắc nghẽn không chính xác, sức mạnh không thích hợp, hoặc động cơ không an toàn. Kiểm tra phần cứng lạ trước.

### Đầu tiên phóng

1. Mở `gwgui.exe`.
2. Mở **Tùy chọn**.
3. Vào **Điều khiển và lái xe** quét và cấu hình ổ đĩa.
4. Kiểm tra hoặc chọn đường dẫn tới `gw.exe`.
5. Vào **Máy** Hãy chọn động cơ nào nên thực hiện mỗi thao tác.
6. Trở lại cửa sổ chính và chọn trang hoạt động cần thiết.

### Xác nhận đã sẵn sàng.

Một thiết lập làm việc nên hiển thị bộ điều khiển và ổ đĩa trong thanh trạng thái, ví dụ một số ổ đĩa, kích cỡ, mật độ và COM cảng. Vào **Tùy chọn > Điều khiển và lái xe ** Cần phải đánh dấu bộ điều khiển **Sẵn sàng ** Và lái xe ** Cấu hình **Chạy đi. ** Thông tin về Controller** trước khi đọc các phương tiện truyền thông có giá trị nếu bạn muốn kiểm tra thông tin liên lạc mà không thay đổi đĩa.

### Chọn một động cơ

GW GUI có thể phơi bày nhiều hơn một thực hiện cho một số hoạt động. Hạt **Greaseweazle Host Tools** Comment `gw.exe`; nội bộ GW GUI Các điều khiển động cơ hỗ trợ thao tác bên trong ứng dụng. Chọn động cơ rõ ràng và độc lập để đọc, viết, chuyển đổi và Disk ExplorerNếu một thao tác không được máy đã chọn hỗ trợ, GW GUI báo cáo rằng điều kiện thay vì tự động thay đổi động cơ.

## Cửa sổ chính

Các nhóm cửa sổ chính

- **Đọc** tạo ảnh từ đĩa vật lý.
- **Ghi** viết ảnh cho đĩa vật lý.
- **Chuyển đổi** Chuyển đổi định dạng hình ảnh đĩa thành một hay nhiều định dạng xuất khác.
- **Hình ảnh hoá** Hiển thị dấu vết, luồng hay dữ liệu đã giải mã.
- **Disk Explorer** Duyệt tập tin được hỗ trợ và nội dung ổ đĩa.
- **Công cụ** cung cấp các lệnh bảo trì phần cứng và chẩn đoán.
- **Mô phỏng** Quản lý và chạy để tiết kiệm máy tính mô phỏng.

Bàn điều khiển ở dưới cho thấy lệnh được thi hành và kết quả. Thanh trạng thái báo cáo ổ đĩa, hồ sơ và tình trạng hiện thời.

### Đang đọc giao diện

Phần lớn các trang thao tác theo cùng một mẫu:

1. **Nguồn hay đích** Điều khiển nhận diện đĩa, ảnh hay thư mục.
2. **Điều khiển Định dạng** chọn phát hiện tự động hoặc một máy và định dạng rõ ràng.
3. **Điều khiển hồ sơ** áp dụng thiết lập có thể sử dụng lại.
4. **Thiết lập cấp cao** Để lộ thông số thường tùy chọn.
5. **Thực hiện** Bắt đầu phẫu thuật.
6. Hạt **bàn điều khiển** hiển thị lệnh tạo, tiến trình, cảnh báo và lỗi.

Hạt **Thực hiện** Nút không ngụ ý rằng mọi giá trị an toàn cho đĩa đã chèn. Luôn luôn xem lại điểm đến và chọn ổ đĩa trước khi thao tác ghi hoặc bảo trì.

### Thanh trạng thái và bàn điều khiển

Bên trái thanh trạng thái cho biết ổ đĩa hoạt động. Trung tâm hiển thị hồ sơ hoạt động khi chọn. Chỉ số bang báo cáo ứng dụng đã sẵn sàng hay đang bận. Bảng điều khiển không chỉ đơn thuần là chẩn đoán: nó là hồ sơ có thẩm quyền của lệnh được gửi cho cơ chế đã chọn. Khi cần bảo tồn hoặc chia sẻ mệnh lệnh này, hãy dùng khả năng kiểm soát bản sao.

## Đọc đĩa

Mở cửa **Đọc** Thẻ để lấy đĩa mềm vật lý làm ảnh.

<p align="center"><img src="images/main-read-en.png" alt="Thẻ đọc" width="78%"></p>

### Thủ tục cơ bản

1. Chèn đĩa nguồn vào ổ đã cấu hình.
2. Chọn kiểu ảnh:
   - **Ảnh thô (SCP)** Bảo tồn thông tin cấp độ flux.
   - **Định dạng đĩa đã biết** tạo ảnh dùng máy và định dạng đã chọn.
3. Chọn thư mục đích.
4. Nhập tên tập tin xuất.
5. Chọn hồ sơ nếu cần thiết.
6. Ấn **Thực hiện**.

Bàn điều khiển cho thấy lệnh và tiến trình chính xác. Không gỡ bỏ đĩa hoặc ngắt kết nối điều khiển cho đến khi thao tác hoàn tất.

### Đang chọn kiểu kết xuất

Dùng **Ảnh thô (SCP)** Khi mục tiêu được ghi nhận, phân tích, phục hồi hoặc sau này cải đạo. Một hồ sơ hình ảnh thô lưu thông tin thời gian và nhiều cuộc cách mạng, hữu ích cho các định dạng khác thường, khu vực yếu, kế hoạch bảo vệ và phương tiện truyền thông bị hỏng.

Dùng **Định dạng đĩa đã biết** Khi bạn đã biết gia đình đĩa và cần một hình ảnh khu vực có thể sử dụng trực tiếp. Lựa chọn này có thể nhỏ hơn và dễ mở hơn trong phần mềm khác, nhưng nó đại diện cho kết quả đã được giải mã thay vì mỗi chi tiết được hiển thị bởi ổ đĩa.

Khi không chắc chắn, trước tiên hãy tạo ra hình ảnh thô. Bạn có thể chuyển đổi nó sau này mà không cần đọc lại đĩa.

### Thư mục, tên tập tin và hồ sơ

Hạt **Thư mục ** là thư mục đích. Hạt ** Tên tập tin** nên xác định ổ đĩa mà không dựa vào nhãn hiệu vật lý của nó. Một tên phổ có ích chứa tựa đề, số lượng hoặc cạnh đĩa và một ghi chú thích hợp. Đừng thêm phần mở rộng định dạng mâu thuẫn với định dạng xuất đã chọn.

A **Hồ sơ ** áp dụng một bộ tham số đọc đã lưu. Chọn một khi bạn biết nó chứa gì. Hạt ** Mặc định** Hồ sơ cá nhân thích hợp cho một nỗ lực đầu tiên bình thường; một hồ sơ phục hồi đặc biệt có thể cố tình đọc nhiều cuộc cách mạng hơn hoặc một phạm vi khác và do đó mất nhiều thời gian hơn.

### Thiết lập cấp cao

Mở rộng **Thiết lập cấp cao** truy cập các thông số định dạng đặc trưng hoặc các tham số chuyên gia. Để các giá trị này không thay đổi trừ khi đĩa đòi hỏi một phạm vi theo dõi cụ thể, số đếm cách mạng, hoặc tùy chọn điều khiển.

Giá trị cấp cao thông thường bao gồm:

| Thiết lập | Mục đích | Khi nào thay đổi nó |
|---|---|---|
| Vùng vẽ | Giới hạn các xi lanh và đầu đọc | Phương tiện đơn phương, hình học bất thường, hoặc thẻ thông hành phục hồi mục tiêu |
| Cách mạng | Name | Tăng tốc độ cho các đường ray không ổn định hoặc được bảo vệ; giảm chỉ cho tốc độ khi thích hợp |
| Đối số cao | Name | Chỉ khi làm theo tài liệu Greaseweazle hướng dẫn |

### Kiểm tra đọc thành công

Không chỉ dựa vào sự vắng mặt của hộp thoại lỗi. Sau khi lệnh hoàn tất:

1. Xác nhận tập tin xuất tồn tại và không rỗng.
2. Hãy đọc những dòng điều khiển cuối cùng cho những dấu vết bị lỗi hoặc bị thiếu.
3. Mở ảnh trong **Hình ảnh hoá** để kiểm tra rằng cả hai bên và theo dõi mong đợi dải chứa dữ liệu.
4. Mở ra **Disk Explorer** khi hỗ trợ hệ thống tập tin.
5. Giữ bản ghi chiến dịch với những ghi chép quan trọng.

Nếu đọc đi đọc lại khác, hãy giữ cho mỗi đoạn bắt sống thay vì viết quá nhiều. Sự khác biệt có thể hữu ích trong quá trình hồi phục.

## Viết đĩa

Mở cửa **Ghi** Thẻ để ghi ảnh đã có vào đĩa mềm vật lý.

<p align="center"><img src="images/main-write-en.png" alt="Ghi" width="78%"></p>

### Thủ tục cơ bản

1. Chèn đĩa đích.
2. Chọn ảnh gốc bằng **Duyệt**.
3. Xác nhận định dạng phát hiện.
4. Chọn hồ sơ nếu cần thiết.
5. Ấn **Thực hiện**.

Viết thay thế dữ liệu trên đĩa đích. Kiểm tra ổ đĩa và ảnh đã chọn trước khi khởi động.

> **Cảnh báo:** Viết thì phá hoại. Nó thay thế dữ liệu từ tính trên đĩa đích. Dùng kho lưu trữ mã nguồn đã ghi và đĩa đích riêng khi có thể.

### Trước khi viết

Kiểm tra bốn mục trước khi nhấn **Thực hiện**:

1. **Ảnh:** đường dẫn đã chọn là ảnh gốc đã định.
2. **Đĩa:** Ổ đĩa trong ổ đĩa có thể được ghi đè an toàn.
3. **Lái:** kích cỡ và mật độ được cấu hình phù hợp với trung tâm đích.
4. **Định dạng:** Phát hiện tự động hoặc định dạng đã chọn bằng tay khớp với ảnh.

Nếu ảnh gốc chưa được thử nghiệm, mở nó vào **Hình ảnh hoá ** hay ** Disk Explorer** Đầu tiên. Không thể sửa chữa ảnh mã nguồn không đầy đủ.

### Theo dõi kiểm tra và sửa đổi

Sau khi chọn ảnh, **Hình ảnh hoá ** Mở trình diễn đường ray. ** Sửa đổi** Hiển thị các sửa đổi ảnh được hỗ trợ trước khi ghi. Hành động sẵn sàng phụ thuộc vào định dạng và cơ chế đã chọn.

### Kiểm tra một đĩa đã ghi

Khi động cơ hỗ trợ xác minh, hãy dùng nó cho phương tiện truyền thông quan trọng. Nếu không, hãy đọc đĩa viết trở lại một hình ảnh mới và so sánh nội dung đã được giải mã hoặc kiểm tra nó trong **Hình ảnh hoá** Hãy giữ cho việc thu dữ liệu được tách biệt với ảnh gốc để bản gốc không bao giờ bị ghi đè.

Nếu việc ghi bị lỗi tại dấu vết nhất định, hãy kiểm tra tình trạng đĩa, mật độ, sự sạch sẽ và cấu hình ổ đĩa. Nếu thất bại xảy ra ngẫu nhiên, kiểm tra USB Sự ổn định và kiểm soát giao tiếp.

## Đang chuyển đổi ảnh đĩa

Hạt **Chuyển đổi** Thẻ sẽ chuyển đổi ảnh gốc thành một hay nhiều định dạng đích.

<p align="center"><img src="images/main-conversion-en.png" alt="Thẻ đảo ngược" width="78%"></p>

### Thủ tục cơ bản

1. Chọn ảnh gốc.
2. Tùy chọn cung cấp tên xuất.
3. Chọn một gia đình máy móc.
4. Chọn một hay nhiều định dạng đầu ra và phần mở rộng.
5. Bật **Thêm** nếu tên tập tin dùng mẫu thẻ đã cấu hình.
6. Ấn **Thực hiện**.

Hạt **Đã chọn ** bảng liệt kê các kết xuất đã yêu cầu. ** Di trú tập tin** cung cấp luồng công việc tận tụy cho việc di chuyển tập tin được hỗ trợ thay vì chuyển đổi ảnh tiêu chuẩn.

### Chọn định dạng

Hạt **Máy:** danh sách bộ lọc các định dạng được hiển thị trong ** Định dạng** bảng điều khiển. Tên định dạng mô tả bố trí đĩa hợp lý; phần mở rộng mô tả hộp xuất. Một số định dạng có thể được biểu thị bằng hơn một phần mở rộng, và một số thùng không thể bảo tồn mọi đặc điểm của một nguồn nguyên.

Chọn chỉ kết xuất thực sự cần thiết. Nhiều định dạng rất hữu ích khi tạo ra một tổng hợp người chủ, một bản sao giả lập, và bản sao cho một công cụ phân tích khác trong một hoạt động.

### Tên và thẻ xuất

**Tên xuất ** Cho bạn khả năng điều khiển các tên cơ bản được tạo ra cho định dạng đã chọn. ** Thêm ** áp dụng mẫu tên tập tin được cấu hình ** Tùy chọn > Chung**Thẻ có thể mã hóa họ, định dạng, mở rộng, ngày tháng hoặc giờ. Xem thử ví dụ về tùy chọn trước khi chuyển đổi một mẻ lớn để đặt tên tập tin luôn luôn.

### Đang kiểm tra kết quả chuyển đổi

Cho mỗi kết xuất đã yêu cầu:

1. Xác nhận rằng một tập tin đã được tạo ra.
2. Kiểm tra bàn điều khiển tìm dấu vết hay khu vực không thể giải mã.
3. Mở kết quả ra **Disk Explorer** nếu nó chứa hệ thống tập tin được hỗ trợ.
4. So sánh khả năng và nội dung của đĩa với nguồn.

Sự cải đạo có thể hoàn tất trong khi báo cáo sự mất thông tin vốn có trong định dạng đích đến. Giữ lại ảnh gốc ngay cả khi ảnh được sửa đổi có vẻ đúng.

## Hiển thị ảnh đĩa

Hạt **Hình ảnh hoá** Thẻ hiển thị cấu trúc và phân phối dữ liệu của ảnh.

<p align="center"><img src="images/main-visualization-en.png" alt="Thẻ hình ảnh hoá" width="78%"></p>

1. Ấn **Mở ảnh đĩa**.
2. Giữ **Phát hiện tự động** đã bật, hoặc chọn máy và định dạng tự.
3. Dùng **Phóng đại liên kết** để giữ cho cả hai bên ở cùng mức độ phóng đại.
4. Dùng **Đặt lại** để khôi phục lại ô xem ban đầu.
5. Mở **Thanh tra** để biết chi tiết về vùng được chọn.

Truyền thuyết phân biệt luồng thông thường, chuyển tiếp ngắn và dài, đầu đề, dữ liệu giải mã, và phát hiện dị thường. Một ảnh thô có thể chứa dữ liệu không thể giải mã thành hệ thống tập tin đã biết, nhưng vẫn có thể được kiểm tra ở đây.

### Giải thích phong cảnh

Mỗi bảng hình tròn lớn đại diện cho một mặt đĩa. Trung tâm nhận diện phần bên và trạng thái dữ liệu hiện tại của nó; vị trí đồng tâm tương ứng với dấu vết. Tô màu những vùng được phát hiện theo truyền thuyết. Hình ảnh hóa nhằm trả lời các câu hỏi như:

- Ảnh chứa dữ liệu ở một bên hay cả hai bên?
- Dấu vết mong đợi có mặt không?
- Các dị thường có bị cô lập hoặc lặp đi lặp lại trên đĩa không?
- Phát hiện tự động có xác định được một loại máy và định dạng hợp lý không?

Một màu bất thường là một lý do để kiểm tra khu vực, chứ không phải chứng minh rằng đĩa là không thể sử dụng được. Bảo vệ sao chép, định dạng không chuẩn, ghi âm yếu, và một khu vực bị hỏng có thể tạo ra những cấu trúc khác nhau cần sự giải thích ngữ cảnh.

### Bộ kiểm tra khuyến cáo

Bắt đầu với phóng đại liên kết có khả năng so sánh hai bên ở cùng một quy mô. Chọn vùng khả nghi, mở **Thanh tra** và so sánh nó với dấu vết hàng xóm. Nếu kết quả xuất hiện là một vấn đề phát hiện, tắt tự động phát hiện và chọn một máy và định dạng đã biết. Trở lại để phát hiện tự động sau khi kiểm tra để không vô tình sử dụng thiết lập ép buộc cho một hình ảnh khác.

## Nổ tung nội dung ổ đĩa

Hạt **Disk Explorer** Duyệt thẻ hỗ trợ ảnh đĩa dạng phân cấp tập tin.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer Thẻ" width="78%"></p>

1. Mở ảnh đã có hoặc đọc đĩa.
2. Giữ **Phát hiện tự động** đã bật, trừ khi bạn cần phải ép buộc máy hay định dạng.
3. Xem lại thông tin âm lượng: hệ thống, bảo vệ, hệ thống tập tin, khả năng, không gian miễn phí và số đếm mục.
4. Duyệt thư mục trong bảng bên trái.
5. Chọn mục cần xem chi tiết của nó trong bảng bên phải.

Nếu định dạng ảnh hay hệ thống tập tin không được hỗ trợ, hãy dùng **Hình ảnh hoá** để kiểm tra cấu trúc thô thay thế.

### Hiểu các bảng

Bản tóm tắt trên cùng mô tả hình được gắn và phát hiện âm lượng. Bảng dưới bên trái chứa phân cấp thư mục. Bảng trung tâm liệt kê các mục trong thư mục đã chọn với tên, ngày tháng, kiểu và kích cỡ. Bảng bên phải hiển thị chi tiết về mục đã chọn.

Disk Explorer không có nghĩa là mọi dấu vết sống đều được giải mã hoàn hảo. Dùng bản tóm tắt âm lượng và số mục như một kiểm tra nhanh tính khả thi, rồi mở tập tin đại diện hoặc so sánh với danh sách thư mục đã biết khi bảo tồn tính chính xác.

### Khi không có gì xuất hiện

Đầu tiên xác nhận đường dẫn của ảnh là đúng. Sau đó kiểm tra máy và định dạng. Một ảnh hợp lệ có thể chứa một hệ thống tập tin không được hỗ trợ hay bị hư hỏng, trong trường hợp nhà thám hiểm có thể vẫn rỗng mặc dù **Hình ảnh hoá** hiển thị dữ liệu ghi âm. Đừng ghi đè lên hoặc bỏ ảnh gốc chỉ dựa vào nhà thám hiểm rỗng.

## Dùng công cụ

Hạt **Công cụ** Nhóm thẻ Greaseweazle Hoạt động bảo trì.

<p align="center"><img src="images/main-tools-en.png" alt="Thanh công cụ" width="78%"></p>

Chọn một lệnh từ danh sách bên trái, xem lại các tham số của nó, rồi nhấn vào **Thực hiện** Chỉ nên dùng lệnh hủy bỏ hay thay đổi phần cứng sau khi kiểm tra lại bộ điều khiển và ổ đĩa đã chọn.

Phần lớn hộp thoại công cụ chứa ba vùng: tham số ở trên cùng, trạng thái và khu vực đầu ra ở giữa, và lệnh tạo ra ở dưới. Xem thử lệnh đã bật tùy chọn. Một tham số không được kiểm duyệt thường có nghĩa là “đừng sửa đổi giá trị này, trong khi tham số đã kiểm tra bao gồm giá trị đó trong lệnh.

Hộp thoại chuẩn đoán cá nhân được mô tả trong [Những chẩn đoán và bảo trì phần cứng](#hardware-diagnostics-and-maintenance).

## Mô phỏng

### Mở một máy lưu

Hạt **Mô phỏng ** Danh sách thẻ đã lưu cấu hình. Chọn một và nhấn ** Mở**Mỗi máy chạy xuất hiện trên trang riêng của nó.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Comment" width="78%"></p>

Tạo và chỉnh sửa máy **Tùy chọn > Mô phỏng > Cấu hình ** và ** Tùy chọn > Mô phỏng > Amiga**.

Nếu không có cấu hình, hãy tạo một trong các tùy chọn trước. Một cấu hình đã lưu kết hợp mô hình máy tính, phiên bản giả lập, ROMBộ nhớ, video, âm thanh, lưu trữ và bản đồ đầu vào. Lưu một cấu hình không khởi chạy; quay về chính **Mô phỏng ** tab và nhấn ** Mở**.

### Điều khiển máy chạy

<p align="center"><img src="images/main-emulation-running-en.png" alt="Đang chạy máy mô phỏng" width="78%"></p>

Thanh công cụ chạy máy cung cấp năng lượng, tạm dừng, đặt lại, lưu trữ trạng thái, tải, thu và điều khiển hiển thị. Nó cũng cho thấy:

- đã cấu hình nhanh và tải nhanh các phím tắt;
- Trình vẽ hoạt động, chẳng hạn Direct3D 11;
- đầy màn hình và đường tắt thả chuột;
- Âm thanh, điều khiển và trạng thái chuột;
- độ phân giải, tốc độ cập nhật và tỷ lệ khung.

Dải đĩa ở dưới cùng của màn hình mô phỏng quản lý các phương tiện di chuyển được cho mỗi ổ đĩa mô phỏng. Name **Tùy chọn > Mô phỏng > Phím tắt** Trong khi mô phỏng bản đồ bàn phím, chuột và điều khiển được cấu hình trong tương ứng Amiga duyệt.

### tham chiếu về thanh công cụ

| Nhóm điều khiển | Mục đích |
|---|---|
| Name | Khởi động, dừng, tạm dừng, hoặc tiếp tục máy mô phỏng |
| Đặt lại điều khiển | Thực hiện hành động đặt lại mềm hay cứng được cấu hình |
| Điều khiển trạng thái | Lưu hay nạp một trạng thái giả lập cho việc tiếp tục nhanh |
| Thu | Lưu ảnh của bộ trình bày mô phỏng |
| Hiển thị | Thay đổi cách trình bày màn hình hoặc nhập đầy màn hình |
| Nhắc nhở bang nhanh | Hiện cách gõ tắt/ Tải lại |
| Vẽ | Báo cáo hậu phương phim hoạt động |
| Nhắc nhở nhập | Hiện đường tắt toàn màn hình và con chuột |
| Chỉ thị thiết bị | Báo cáo âm thanh, điều khiển và tình trạng chuột |
| Hiệu suất | Thông báo kích cỡ kết xuất, tần số cập nhật và tần số khung |

### Để lại đầy màn hình hoặc thả chuột

Thanh công cụ hiển thị các khóa được chỉ định hiện thời. Trong cấu hình minh họa, **Alt+ Trả lại ** bật/tắt toàn màn hình và ** F12** Thả chuột ra. Xử lý các giá trị đã hiển thị là có thẩm quyền vì có thể gán lại cách gõ tắt.

### Dùng phương tiện mềm

Các dải ổ đĩa nhận diện mỗi ổ đĩa mô phỏng, như `DF0:`.. Dùng điều khiển phương tiện để chèn, thay thế hoặc phóng ra ảnh. Thay đổi phương tiện truyền thông chỉ thay đổi đĩa cắm của máy đang chạy; nó không thay đổi định nghĩa lưu trữ trong bộ máy đã lưu trừ khi nó được lưu rõ ràng.

## Tùy chọn ứng dụng

Mở **Tùy chọn** từ cửa sổ chính để cấu hình ứng dụng.

### Chung

<p align="center"><img src="images/options-general-en.png" alt="Tùy chọn chung" width="72%"></p>

Hạt **Chung** Thẻ chứa:

- Thư mục ảnh đĩa mặc định;
- Ngôn ngữ giao diện và sắc thái;
- Thế hệ thẻ tên để chuyển đổi;
- Các kiểu thẻ tự chọn có sẵn và gần đây;
- một ví dụ tên tập tin sống.

Biến thẻ bao gồm tên nguồn, gia đình, định dạng, phần mở rộng, ngày và giờ. Dùng cái nút đặt lại để phục hồi mẫu mặc định.

Xem thử tên tập tin cập nhật trước khi tạo tập tin. Dùng nó để phát hiện dấu định giới bản sao, phần mở rộng bị thiếu, hoặc tên mơ hồ. Các mẫu tự chọn gần đây cung cấp truy cập nhanh các bộ màu tên trước đó mà không thay thế thiết lập sẵn hiện thời.

### Bản ghi

<p align="center"><img src="images/options-logs-en.png" alt="Tùy chọn đăng nhập" width="72%"></p>

Ghi lưu có thể được cấu hình độc lập cho mỗi thao tác. Đối với mỗi loại, hãy chọn có nên lưu bản ghi, đặt kích cỡ tập tin tối đa, và quyết định có nên giữ lại bản ghi trước hay không. Cỡ `0` có nghĩa là vô hạn. **Mở thư mục** mở thư mục bản ghi hiện thời.

Bật **Giữ bản ghi trước** Để bảo tồn và chẩn đoán, lịch sử của nhiều nỗ lực có vấn đề. Tắt nó khi chỉ kết quả gần đây nhất có ích. Giới hạn cỡ tối đa áp dụng cho kho lưu, không phải cho ảnh đĩa đã chụp.

### Điều khiển và lái xe

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Điều khiển và lái xe" width="72%"></p>

Dùng thẻ này:

- quét tìm bộ điều khiển kết nối;
- thêm và gỡ bỏ cấu hình ổ đĩa;
- chọn kích cỡ ổ đĩa, mật độ và tốc độ;
- lưu thiết lập phần cứng;
- chọn hay tự động tìm `gw.exe`;
- kiểm tra và tải về Greaseweazle Host Tools Cập nhật;
- Phục hồi một đường dẫn thực hiện được cấu hình trước.

Thiết lập phần cứng đã lưu vẫn còn sẵn sàng khi ổ đĩa tạm thời bị ngắt kết nối.

#### Thêm ổ đĩa

1. Ấn **Quét** và đợi người điều khiển kết nối xuất hiện.
2. Ấn **Thêm ổ đĩa** nếu ổ đĩa cần thiết chưa được liệt kê.
3. Chọn số lượng động cơ hợp lý, kích thước vật lý, mật độ ghi âm và tốc độ quay.
4. Cứu hàng rào.
5. Xác nhận rằng nó hiển thị **Sẵn sàng ** và ** Cấu hình**.

Dùng bộ điều khiển thùng rác chỉ để gỡ bỏ cấu hình đã lưu; nó không ngắt phần cứng. Nếu cùng một bộ điều khiển xuất hiện trên một khác COM Sau đó, hãy quét lại trước khi giả sử cổng đã được lưu trữ vẫn còn hiệu lực.

#### Quản lý Greaseweazle Host Tools

**Tìm gw.exe ** Tìm kiếm vị trí đã biết. ** Chọn ** chọn một tập tin thực hiện được đặc biệt. ** Kiểm tra cập nhật ** Các phiên bản có sẵn mà không thay thế phiên bản đã cài đặt. ** Tải phiên bản mới nhất ** cài đặt gói hiện thời, và ** Dùng đường dẫn trước ** khôi phục vị trí đã được cấu hình trước đó. Sau khi thay đổi tập tin thực hiện, chạy ** Thông tin về Controller** để xác nhận rằng phiên bản đã chọn có thể liên lạc với bộ điều khiển.

### Máy

<p align="center"><img src="images/options-engines-en.png" alt="Chọn cơ chế" width="72%"></p>

Chọn động cơ một cách độc lập để đọc, viết, chuyển đổi và Disk Explorer.. Máy đã chọn được dùng hoàn toàn: nếu nó không thể thực hiện thao tác đã yêu cầu, GW GUI báo cáo giới hạn thay vì lặng lẽ chuyển động cơ.

Sự độc lập này là cố ý. Thí dụ, đọc sách về thể chất có thể dùng Greaseweazle Host Tools Trong khi chuyển đổi hình ảnh và khám phá sử dụng động cơ nội bộ. Ghi lại các lựa chọn của động cơ trong hồ sơ hoặc dự án lưu ý khi tính khả thi quan trọng.

### Hồ sơ

<p align="center"><img src="images/options-profiles-en.png" alt="Hồ sơ" width="72%"></p>

Phân tích lưu thiết lập có thể sử dụng lại cho thao tác đọc, ghi và chuyển đổi. Chọn phân loại thích hợp để quản lý hồ sơ của nó. Một hồ sơ đã chọn được hiển thị trong thanh trạng thái chính và trong màn hình thao tác.

Dùng hồ sơ cho luồng làm việc lặp đi lặp lại thay vì bộ sưu tập các lá cờ chuyên gia. Cung cấp cho mỗi hồ sơ một tên cụ thể mục đích, chẳng hạn như ổ đĩa, gia đình đĩa hoặc phương pháp phục hồi. Xem lại hồ sơ sau khi cập nhật cơ chế cơ chế cơ bản vì các tùy chọn được hỗ trợ có thể thay đổi.

## Tùy chọn tạo

Hạt **Mô phỏng** tùy chọn chứa thiết lập lưu trữ chung, gõ tắt toàn cục, cấu hình đã lưu và thiết lập riêng của máy.

### Đang đồng bộ thư mục

<p align="center"><img src="images/options-emulation-general-en.png" alt="Tùy chọn phân cách chung" width="72%"></p>

Đặt thư mục lưu tạm và thư mục mặc định cho việc bắt và lưu các trạng thái. **Mở thư mục** mở vị trí chung trong Tập tin Explorer.

Giữ lấy và lưu các trạng thái trong mỗi thư mục riêng. Chụp là một hình ảnh bình thường; trạng thái được lưu chứa trạng thái giả lập máy tính cụ thể và có thể phụ thuộc vào phiên bản giả lập và cấu hình tạo nó. Lùi lại cấu hình và truyền thông cùng với các bang quan trọng được lưu.

### Từ nóng toàn cục

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Cách gõ tắt" width="72%"></p>

Tìm kiếm một hành động hoặc nhiệm vụ then chốt, chỉ định hoặc gỡ bỏ từ nóng, phục hồi mặc định và xung đột rõ ràng. Cột trạng thái xác định các bài tập hợp lệ và mâu thuẫn nhau.

Để thay đổi phím tắt, hãy tìm hành động, nhắp vào **Gán ** và nhấn mã khóa mong muốn. Kiểm tra tình trạng trước khi đóng tùy chọn. **Xoá xung đột ** gỡ bỏ các bài tập mâu thuẫn; nó không phục hồi bản đồ mặc định. Dùng ** Phục hồi mặc định** khi bạn muốn thay thế tác vụ tự chọn bằng thiết lập chuẩn.

### Cấu hình đã lưu

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Lưu cấu hình" width="72%"></p>

Trang này liệt kê các máy đã lưu. Chọn một cấu hình để sửa đổi nó trong **Amiga** Thẻ. Bạn có thể thay đổi danh sách hoặc xoá cấu hình đã chọn.

Đang xóa một cấu hình gỡ bỏ định nghĩa máy đã lưu. Nó không nên được sử dụng như một cách để đẩy ra truyền thông hoặc đóng máy đang chạy. Trước khi xoá, lưu ý bất kỳ ROMảnh đặc và tập tin trạng thái liên quan đến cấu hình.

## Amiga cấu hình

Giao diện hiện thời cung cấp chi tiết Amiga trang cấu hình. Cấu trúc thiết lập tương tự có thể mở rộng cho các hệ thống mô phỏng khác mà không thay đổi dòng chảy chính.

### Chung

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga Thiết lập chung" width="72%"></p>

Chọn Amiga Mô hình, lưu cấu hình, cài đặt hoặc thay thế phiên bản giả lập, và xác định thư mục mặc định cho đĩa cứng và các phương tiện khác. **Phiên bản tìm kiếm** Yêu cầu mã nguồn giả lập chính thức.

Bắt đầu với mô hình vì nó ép các trang sau. Thay đổi nó có thể thay đổi tình hình hiện có CPUTrí nhớ, ROM... và những lựa chọn lưu trữ. Sau khi chọn phiên bản mô phỏng, hãy lưu cấu hình trước khi phóng nó ra cửa sổ chính. Cài đặt phiên bản mô phỏng khác thay thế phiên bản được dùng bởi cấu hình đó; nó không tạo bản sao thứ hai của máy.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU thiết lập" width="72%"></p>

Hạt CPU trang hiển thị bộ xử lý được chọn bởi mô hình máy và cung cấp độ chính xác tương thích, FPUvà sự lựa chọn tốc độ. Tùy chọn không áp dụng cho mô hình đã chọn vẫn bị tắt.

- **CPU Mô hình** Xác định trình xử lý mô phỏng.
- **Độ chính xác** Điều khiển mô hình thời gian. Chế độ phân giải vòng lặp ưu tiên phần cứng tương thích nhưng yêu cầu thêm máy xử lý.
- **FPU** hiệu lực đơn vị nổi phù hợp khi được hỗ trợ.
- **CPU Tốc độ** chọn thời gian gốc hoặc chế độ tăng tốc.

Để cấu hình một đường thẳng cơ bản, hãy giữ cho mô hình-mô hình CPU và tốc độ ban đầu. Thay đổi gia tốc chỉ sau khi khởi động máy đúng tại thiết lập chuẩn.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM thiết lập" width="72%"></p>

Cấu hình Chip RAMChậm RAMNhanh RAMvà hỗ trợ bộ nhớ mở rộng. Thông điệp tương thích giải thích những hạn chế cho máy đã chọn, và tổng bộ nhớ đã cấu hình được hiển thị ở dưới cùng.

**Chip RAM ** có thể truy cập vào thẻ tự chọn và được yêu cầu bởi nền tảng. ** Chậm RAM ** đại diện bộ nhớ mở rộng tương thích với cấu hình chung. ** Nhanh RAM ** là bộ nhớ mở rộng xử lý. ** GenericName RAM** Chỉ áp dụng cho những mô hình ủng hộ sự mở rộng kiến trúc. Thông điệp tương thích và điều khiển bị tắt ngăn chặn kết hợp mà mô hình đã chọn không thể đại diện.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM thiết lập" width="72%"></p>

Chọn khởi chạy cú pháp hệ thống ROM, mở rộng tùy chọn ROM, và ROM Chìa khóa. Phát hiện...ROM Danh sách hiển thị tên, sửa đổi và tương thích với mô hình đã chọn. Chọn một phát hiện ROM rồi nhấn **Dùng** hoặc duyệt qua tập tin bằng tay.

ROM tập tin không được cung cấp bởi GW GUIDùng ROM hợp pháp để dùng.

Danh sách được phát hiện là thích hợp hơn để đoán từ tên tập tin: nó báo cáo ROM Danh tính, chỉnh sửa và đánh giá tương thích với mô hình đã chọn. **Tương thích ** là sự lựa chọn bình thường; ** Tương thích một phần ** Cho thấy rằng ROM Có thể khởi động nhưng không chính xác tương ứng với máy. ** Cập nhật ** Name ROM Vị trí. ** Dùng** chỉ định thiết bị phát hiện đã chọn ROM để cấu hình.

### Ảnh động

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga Thiết lập ảnh động" width="72%"></p>

Cấu hình tỷ lệ hình thể, tỷ lệ hình thể, độ phân giải, chế độ đường kẻ, độ cắt biên, độ sâu màu, khung bỏ qua, gamma, và nhấp nháy sửa chữa. Thiết lập chip phụ thêm sẵn sàng dưới trang này khi được hỗ trợ bởi mô hình đã chọn.

| Thiết lập | Hiệu ứng thiết thực |
|---|---|
| Tiêu chuẩn ảnh động | Chọn PAL hay NTSC Name |
| Tỷ lệ Hình thể | Điều khiển cách mà bức tranh được mô phỏng được quy mô hóa |
| Độ phân giải | Chọn chi tiết kết xuất tự động hay rõ ràng |
| Chế độ Đường | Điều khiển kết xuất hai dây hay hai chiều |
| Cắt viền | Gỡ bỏ danh sách không dùng chỉ khi bật |
| Vẽ | Chọn hậu phương đồ hoạ |
| Độ sâu màu | Chọn độ chính xác màu xuất |
| Bỏ qua khung | Comment |
| Gamma (γ) | Điều chỉnh phản ứng độ sáng |
| Flicker sửa chữa | Các chế độ xử lý khác sẽ nhấp nháy |

Thay đổi thiết lập hiển thị cùng một lúc. Nếu cửa sổ mô phỏng trở nên trống hoặc không ổn định, hãy trở về độ phân giải tự động, khung khuyết tật bỏ qua, khung trung lập gamma, và bộ làm việc trước đó.

### Âm thanh

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga Thiết lập âm thanh" width="72%"></p>

Bật hay tắt âm thanh, chọn thiết bị xuất và độ nhạy, rồi cấu hình độ phân giải, Amiga Bộ lọc, loại lọc, tách âm thanh, âm thanh lái mềm, và âm thanh quảng cáo CD.

Sự chậm trễ giảm thiểu nhưng có thể gây ra sự bỏ học trên một máy tính bận rộn. Tăng cường nếu âm thanh kêu lên. Nội suy và Amiga Bộ lọc âm thanh thay đổi khả năng sinh sản âm thanh hơn là mô phỏng lập trình logic. Name Amiga âm thanh.

### Lưu trữ

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga Thiết lập kho lưu" width="72%"></p>

Trang kho liệt kê thiết bị nhận diện, kiểu, mô hình, phương tiện truyền thông liên quan và hành động sẵn có. Thêm, cấu hình, hoặc gỡ bỏ thiết bị ở đây. Đĩa mềm và CD có thể được chèn hoặc thay thế trực tiếp từ một máy đang chạy.

Hạt **Bộ nhận diện thiết bị ** Đó là cách hệ thống mô phỏng tìm kiếm thiết bị. ** Kiểu ** phân biệt mềm mại, cứng, quang học và các thiết bị hỗ trợ khác. ** Mô hình ** Mô tả phần cứng bắt chước, trong khi ** Phương tiện tương tác** Xác định ảnh đang được gán. Cấu hình thiết bị trước khi kết hợp các phương tiện có khả năng ghi, và giữ các bản sao lưu của ảnh khó gỡ.

### Bàn phím

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga Thiết lập bàn phím" width="72%"></p>

Tìm kiếm Amiga phím và nhiệm vụ máy, chỉ định các phím mới, gỡ bỏ bản đồ, phục hồi mặc định, hoặc rõ ràng xung đột. Báo cáo cột trạng thái có đúng không.

Các tên cột bên trái được mô phỏng Amiga Khóa: **Hợp** hiển thị tổ hợp phím chủ. Một bản đồ hợp lệ vẫn có thể không tiện nếu Windows hay ứng dụng dự trữ cùng cách gõ tắt, vì vậy thử nghiệm các tổ hợp quan trọng bên trong máy đang chạy. Tránh chỉ định cách giải quyết con chuột hoặc đường tắt đầy màn hình cho một phím mà phần mềm mô phỏng cần thường xuyên.

### Chuột

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga Thiết lập con chuột" width="72%"></p>

Đặt tốc độ chân dung của chuột, chọn cây tương tự điều khiển con chuột, điều chỉnh vùng chết và tốc độ tương tự, và cấu hình bản đồ hành động của chuột. Phục hồi mặc định hoặc rõ ràng xung đột bản đồ khi cần thiết.

Tăng khu vực chết lên nếu bộ điều khiển gây ra sự dịch chuyển. Điều chỉnh tốc độ liên kết trái và phải một cách độc lập khi cả hai thanh bật. Bảng bản đồ thấp hơn liên kết máy chủ đầu vào với nút chuột hoặc hành động; thanh tra trạng thái xung đột sau khi thay đổi bản đồ điều khiển ở nơi khác.

### Điều khiển

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga Thiết lập điều khiển" width="72%"></p>

Phát hiện bộ điều khiển kết nối, thiết bị và kiểu điều khiển để Amiga Cổng, và cấu hình bản đồ điều khiển và thiết lập phóng đại. Tùy chọn sẵn sàng phụ thuộc vào phần cứng được phát hiện và máy được chọn.

Cổng 1 và Cổng 2 được cấu hình độc lập. **Tự động** Kiểu điều khiển là điểm khởi động hợp lý, nhưng phần mềm mong đợi một cần điều khiển hoặc con chuột có thể cần một kiểu rõ ràng. Chạy phát hiện trước khi chỉ định bộ điều khiển mới kết nối. Turbo bắn liên tục kích hoạt một đầu vào bản đồ và nên bị tắt trừ khi trò chơi hoặc lợi ích của nó.

## Những chẩn đoán và bảo trì phần cứng

Những hộp thoại này được mở từ **Công cụ ** Thẻ. Mỗi hộp thoại xem thử bản tạo Greaseweazle Chỉ huy. Xem lại trước khi nhấn nút ** Thực hiện**.

### Thông tin về Controller

<p align="center"><img src="images/tool-controller-information-en.png" alt="Thông tin về Controller" width="62%"></p>

Hiển thị thông tin được trình bày bởi bộ điều khiển đã chọn. Mở rộng **Kết xuất thô** Khi bạn cần đáp ứng toàn bộ lệnh.

Dùng cái này làm lệnh chẩn đoán đầu tiên. Một phản ứng thành công xác nhận rằng GW GUI có thể khởi chạy thực hiện được Công cụ Máy đã cấu hình và liên lạc với thiết bị đã chọn. Ghi lại thông tin phần cứng và phần cứng trước khi cập nhật.

### USB băng thông

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB băng thông" width="62%"></p>

Đo lường sẵn sàng USB Băng thông liên lạc. Sử dụng nó để chẩn đoán không ổn định chuyển dịch hoặc không phù hợp USB kết nối.

Đóng phần mềm khác sử dụng bộ điều khiển trước khi thử ra. Lặp lại các đo lường sau khi thay đổi các USB Cổng, cáp, hay trung tâm. Hãy so sánh kết quả trong những điều kiện tương tự thay vì chỉ xem xét một biện pháp để bảo đảm tuyệt đối.

### Tốc độ

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Tốc độ" width="62%"></p>

Đo tốc độ quay. Hãy gia tăng số lượng đo khi bạn cần thêm một kết quả đại diện.

Một phép đo nhỏ là kiểm tra nhanh; một số phép đo cho thấy tốc độ có ổn định hay không. Hãy để ổ đĩa đạt tốc độ bình thường trước khi giải thích kết quả. Một giá trị bất ngờ có thể cho thấy tốc độ được cấu hình sai, một vấn đề cơ khí hoặc một vấn đề thiết lập.

### Tìm đầu

<p align="center"><img src="images/tool-seek-head-en.png" alt="Tìm đầu" width="62%"></p>

Chuyển đầu ổ đĩa tới một trụ đã chọn. **Cho phép hình trụ cực đoan ** Cho phép các vị trí thường bị hạn chế, và ** Giữ động cơ hoạt động** Để động cơ chạy trong lúc phẫu thuật. Chỉ sử dụng những vị trí cực đoan khi quy trình phần cứng rõ ràng yêu cầu chúng.

Tìm kiếm thông thường rất hữu ích để xác nhận chuyển động đầu hoặc định vị trước khi chẩn đoán. Lắng nghe những tác động bất thường lặp đi lặp lại và dừng lại nếu xi lanh yêu cầu không phù hợp với ổ đĩa. Công cụ này không đọc hay xác nhận dữ liệu tại trụ đích.

### Name

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Name" width="62%"></p>

Chạy liên tục đọc cho phân tích hành vi lái xe. Nó hỗ trợ theo dõi chọn lọc, cách mạng và đếm, định dạng giải mã, luồng nguyên, chỉ số, tốc độ, PLL, lực lưỡng, tập đoàn cứng, TG43và các lựa chọn ngược lại. Công việc sắp xếp đòi hỏi sự hiểu biết về phương tiện truyền thông và phần cứng thích hợp.

Bắt đầu với một đĩa tham khảo đã biết và một bộ ghi đè nhỏ nhất. **Name ** định nghĩa các dấu vết và đầu mẫu; ** Cách mạng trên mỗi đường đua ** Điều khiển mỗi khoảng thời gian mẫu; ** Số đọc** quyết định lặp lại. Bật định nghĩa đĩa tự chọn hay giải mã chỉ khi nó khớp với phương tiện tham chiếu. Những lựa chọn như là chỉ số giả, lĩnh vực cứng, PLL Ghi đè, ghim mật độ và TG43 là phần cứng- hoặc định dạng cụ thể và có thể không hợp lệ hoá một so sánh khi dùng không đúng.

### ghim phần cứng

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="ghim phần cứng" width="62%"></p>

Đọc hay thay đổi nút điều khiển được hỗ trợ. Chọn chốt, bật **Thay đổi ** Chỉ khi ghi một giá trị và chọn ** Cấp cao** khi cần thiết bởi thao tác phần cứng đã định.

Với **Thay đổi** bị tắt, lệnh rút lui. Đây là cái mặc định an toàn hơn. Thay đổi một cấp trực tiếp ảnh hưởng đến điều khiển I/O và chỉ nên được thực hiện với chính xác Greaseweazle Tài liệu về phần cứng và dây lái.

### Đặt lại bộ điều khiển

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Đặt lại bộ điều khiển" width="62%"></p>

Đặt lại Greaseweazle Điều khiển. Dùng cái này khi phát hiện bộ điều khiển, nhưng không còn đáp ứng bình thường.

Đợi thao tác đĩa hoạt động xong trước khi khởi động lại. Sau đó, hãy quét lại bộ điều khiển nếu trạng thái kết nối của nó không tự động phục hồi. Khởi động lại không sửa chữa lỗi `gw.exe` đường dẫn hoặc bị ngắt kết nối USB thiết bị.

### Trễ

<p align="center"><img src="images/tool-delays-en.png" alt="Comment" width="62%"></p>

Đọc hoặc thay đổi điều khiển giờ giấc, bao gồm chọn lựa, bước đầu, ổn định, động cơ, tự động bỏ phiếu, ghi thời gian và mặt nạ bị trì hoãn. Chỉ hiệu lực giá trị mà bạn định sửa đổi.

Bỏ chọn các trường để lại giá trị điều khiển tương ứng không thay đổi. Trước khi sửa đổi, hãy ghi lại các giá trị tồn tại. Thay đổi thời gian có thể ảnh hưởng đến mọi hoạt động vật lý sau đó, vì vậy kiểm tra với truyền thông có thể hy sinh và phục hồi các giá trị tốt được biết đến nếu hành vi trở nên không đáng tin cậy.

### Phần cứng

<p align="center"><img src="images/tool-firmware-en.png" alt="Cập nhật phần cứng" width="62%"></p>

Cập nhật phần mềm điều khiển. **Cập nhật nạp khởi động** được đánh dấu một cách rõ ràng là nguy hiểm và nên ở lại khiếm khuyết trừ khi thủ tục phần mềm chính thức đòi hỏi. Đừng ngắt kết nối bộ điều khiển trong khi cập nhật.

Trước khi cập nhật, hãy xác nhận bộ điều khiển kết nối với **Thông tin về Controller** Dùng một cái chuồng ngựa. USB kết nối, và đóng các phần mềm khác có thể truy cập nó. Sau khi hoàn tất, tái kết nối hoặc tái điều khiển và đọc lại thông tin của nó để kiểm tra phiên bản phần mềm đã báo cáo.

## Bản ghi và lịch sử thao tác

Mở lịch sử thao tác để kiểm tra lưu bản ghi bằng thao tác.

<p align="center"><img src="images/operation-history-en.png" alt="Hành động lịch sử" width="68%"></p>

Chọn một bản ghi bên trái để hiển thị nội dung của nó. **Xuất** lưu một bản sao để chẩn đoán hoặc hỗ trợ. Đường dẫn và dòng lệnh có thể chứa tên thư mục cá nhân, vậy ôn lại bản ghi đã xuất trước khi chia sẻ.

Bàn giao tiếp trực tiếp trong cửa sổ chính hiển thị lệnh hiện thời và kết xuất gần đây. Nút copy của nó sao chép văn bản đã hiển thị.

### Đọc nhật ký

Một bản chẩn đoán hữu ích chứa lệnh tạo ra, nhãn thời gian, kết xuất động cơ, và trạng thái cuối cùng. Công việc từ dưới lên: xác định lỗi cuối cùng, rồi xác định vị trí cảnh báo đầu tiên hoặc theo dõi thất bại trước đó. Một thất bại chung sau này thường chỉ là hậu quả của một thông điệp trước đó, cụ thể hơn.

Khi so sánh hai lần cố gắng, hãy kiểm tra xem các điều khiển, ổ đĩa, động cơ, hồ sơ, đường dẫn nguồn, định dạng xuất và các đối số chuyên gia đều giống nhau. Nếu không, một kết quả khác có thể phản ánh sự thay đổi về thiết lập thay vì sự bất ổn định trên đĩa.

## Name

GW GUI giữ dữ liệu người dùng riêng biệt với nhị phân ứng dụng. Tùy thuộc vào gói và chế độ đã chọn, thiết lập, bản ghi, công cụ tải về, thành phần giả lập, thu, trạng thái và cấu hình máy được cất giữ trong ứng dụng `Data` thư mục hoặc trong vị trí người dùng- dữ liệu đã cấu hình.

Trước khi thay thế hoặc di chuyển một cài đặt di động, giữ cho các thư mục toàn bộ ứng dụng kết hợp với nhau và sao lưu các `Data` Thư mục. Không di chuyển tập tin cá nhân từ `lib`Bởi vì ứng dụng này giải quyết các thư viện của riêng mình và bên thứ ba từ cấu trúc đó.

### Nội dung sao lưu đã gợi ý

Hãy nhắc lại những điều sau đây khi chúng quan trọng đối với dòng chảy công việc của bạn:

- Thiết lập và hồ sơ ứng dụng;
- Điều khiển và lái xe định nghĩa;
- Mô phỏng cấu hình;
- ROM đường dẫn và quản lý hợp pháp ROM Bản sao lưu;
- Hình ảnh khó gỡ bỏ và di chuyển được;
- lưu trữ các bang;
- Nhật ký chiến dịch dùng làm hồ sơ bảo tồn.

Hình ảnh đĩa có thể lớn hơn nhiều so với thiết lập. Lưu trữ các bậc thầy đọc sách khi có thể, và làm việc trên bản sao.

## Comment

### Đang nén một đĩa lạ

1. Kiểm tra và làm sạch ổ đĩa bằng thủ tục bảo trì thích hợp.
2. Ghi- bảo vệ đĩa nếu có thể.
3. Chọn **Đọc > Ảnh thô (SCP)**.
4. Dùng tên tập tin mô tả và đọc phạm vi theo dõi bình thường với nhiều cách mạng.
5. Xem lại bảng điều khiển và lưu bản ghi.
6. Kiểm tra cả hai bên trong **Hình ảnh hoá**.
7. Chuyển đổi bản sao thành định dạng phân khu.
8. Name **Disk Explorer** hoặc phần mềm thích hợp.
9. Bảo tồn chủ nhân thô, ghi chép và ghi chú cùng nhau.

### Đang sửa đĩa từ ảnh

1. Kiểm tra hình ảnh và xác nhận định dạng gia đình và định dạng của nó.
2. Chèn một đĩa có khả năng hy sinh hay cố ý ghi được kích cỡ và mật độ đúng.
3. Mở **Ghi** và chọn ảnh.
4. Xác nhận ổ đĩa đã cấu hình và định dạng đã phát hiện.
5. Viết đĩa đi.
6. Đọc nó về hình ảnh xác thực riêng biệt.
7. So sánh nội dung đã giải mã và xem xét các dấu vết đáng ngờ.

### Tạo ra sự bắt chước Amiga

1. Mở **Tùy chọn > Mô phỏng > Cấu hình** và tạo hoặc chọn một máy.
2. Vào **Amiga > Chung** chọn phiên bản mô hình và giả lập
3. Chỉ định sự tương thích, hợp pháp ROM.
4. Giữ mặc định kiểu mẫu cho CPU và RAM trên đôi giày đầu tiên.
5. Cấu hình video và âm thanh với thiết lập tự động bảo mật.
6. Thêm thiết bị lưu trữ và liên kết sao chép hình ảnh truyền thông.
7. Xem lại bàn phím, chuột và điều khiển nhiệm vụ.
8. Lưu cấu hình.
9. Trở về **Mô phỏng ** Chọn, và nhấp vào **Mở**.
10. Chỉ sau khi một khởi động cơ bản thành công, thay đổi gia tốc hoặc thiết lập cấp cao mỗi lần một.

## Danh sách an toàn

Trước **Đọc**:

- đĩa nguồn nằm trong ổ đĩa đúng;
- Nguồn được bảo vệ khi có thể;
- đường dẫn xuất sẽ không ghi đè lên một chủ hiện có;
- hồ sơ và định vị khớp với ổ đĩa.

Trước **Ghi ** hay ** Xóa**:

- đĩa đích có thể bị phá hủy;
- ảnh và ổ đĩa là đúng;
- Kích cỡ và mật độ đĩa là tương thích;
- Không có chủ nhân nào được dùng làm điểm đến.

Trước khi một công cụ thay đổi phần cứng:

- không có thao tác nào khác đang chạy;
- bộ điều khiển đúng được chọn;
- Giá trị hiện tại đã được ghi lại;
- Điều khiển có sức mạnh ổn định và USB Kết nối;
- hành động được hỗ trợ bởi tài liệu phần cứng.

## Name

### Chưa liệt kê bộ điều khiển

1. Nối trực tiếp điều khiển vào máy tính.
2. Mở **Tùy chọn > Điều khiển và lái xe**.
3. Ấn **Quét**.
4. Kiểm tra tình trạng điều khiển và lái cấu hình.
5. Chạy **Thông tin về Controller** Nếu phát hiện thành công nhưng lệnh thất bại.

Nếu nó vẫn không xuất hiện, hãy thử một trực tiếp khác USB Cổng và cáp, rồi quay lại. Kiểm tra quản lý thiết bị Windows tìm thiết bị nối tiếp mới phát hiện. Name GW GUI thường chỉ tới một cổng bận, cấu hình cũ, hoặc vấn đề Công cụ máy; một bộ điều khiển vắng mặt từ các điểm Windows USBhoặc phần cứng.

### `gw.exe` không tìm thấy

Mở **Tùy chọn > Điều khiển và lái xe ** sau đó sử dụng **Tìm gw.exe **, ** Chọn **hoặc ** Tải phiên bản mới nhất**Xác nhận rằng đường dẫn đã phát hiện chỉ tới mục đích Greaseweazle cài đặt.

Sau khi chọn nó, chạy **Thông tin về Controller** Nếu việc đó không thành công trước khi liên lạc với phần cứng, hãy kiểm tra bản ghi để tìm đường dẫn không hợp lệ, tập tin bị thiếu, hoặc phiên bản không thể khởi chạy.

### Name

Mở **Tùy chọn > Máy** và kiểm tra động cơ được giao nhiệm vụ đó. GW GUI không lặng lẽ quay trở lại động cơ khác.

Thiết lập máy riêng: Thay đổi động cơ chuyển đổi không thay đổi việc đọc, viết hay Disk ExplorerMở lại thao tác bị lỗi sau khi lưu tùy chọn và xác nhận lệnh tạo ra trong bảng điều khiển.

### Không nhận ra ảnh

Tắt khả năng phát hiện tự động chỉ khi bạn biết chính xác thiết bị và định dạng. Nếu không, hãy thử **Hình ảnh hoá** Thẻ để kiểm tra ảnh ở cấp thấp hơn.

Kiểm tra xem nguồn gốc có phải là dữ liệu bắt sống, ảnh khu vực, công-ten-nơ nén hay là một tập tin không liên quan với phần mở rộng sai. Đừng bao giờ thay đổi tên của một phần mở rộng chỉ để bắt người ta phát hiện; sự cải đạo phải diễn giải đúng cấu trúc nguồn.

### Comment

Kiểm tra cấu hình đã lưu, phiên bản giả được cài đặt, đã chọn ROMcác đường đi lưu trữ và mô hình tương thích. Xem lại bản ghi ứng dụng cho chi tiết lỗi hoàn chỉnh.

Tạm thời trở về CPU, RAMvideo và kho lưu trữ cho một đường cơ sở đơn giản. Nếu đường cơ bản bắt đầu, hãy khôi phục một thiết lập tùy chỉnh mỗi lần. Một tình trạng được lưu được tạo ra với phiên bản giả lập khác hoặc định nghĩa máy cũng có thể thất bại ngay cả khi khởi động sạch hoạt động.

### Comment

Kiểm tra cả hai **Mô phỏng > Phím tắt** trang và trang nhất định của máy, chuột hoặc bộ điều khiển. Giải quyết bất cứ nhiệm vụ nào mang tính mâu thuẫn nhau.

Nếu con chuột bị bắt, hãy dùng phím nóng phát hành được hiển thị trong thanh công cụ đang chạy. Nếu bộ điều khiển được kết nối sau khi tùy chọn được mở, hãy chạy kiểm soát lại trước khi chỉ định nó.

### Name

1. Đọc đầu ra bàn điều khiển trực tiếp.
2. Mở **Hành động lịch sử** cho bản ghi lưu hoàn toàn.
3. Xác nhận điều khiển đã chọn, ổ đĩa, hồ sơ, cơ chế và đường dẫn tập tin.
4. Xuất bản ghi liên quan nếu cần phải chia sẻ để chẩn đoán.

### Comment

Tăng tần số âm thanh, gần CPU- Ứng dụng mạnh mẽ, và trả lại khung video bỏ qua và tăng tốc đến giá trị trước. Kiểm tra xem thiết bị âm thanh đã định của Windows đã được chọn. Thay đổi một thiết lập mỗi lần vì vậy hiệu quả sửa chữa là nhận diện được.

### Name

Giải quyết trở lại và chế độ dòng để **Tự động**, vô hiệu hoá khung đang bỏ qua và nhấp nháy sửa tạm thời, và thử trình làm việc trước đó. Xác nhận cấu hình ROM và thêm vào truyền thông khởi động là hợp lệ. Hạt FPS Chỉ thị giúp phân biệt một vấn đề hiệu quả với một máy chưa khởi động.

### Name

Lặp lại đoạn đọc cho tên tập tin mới, làm tăng các cuộc cách mạng thích hợp và so sánh các dấu vết bị ảnh hưởng. Làm sạch đầu lái bằng cách xử lý đúng thủ tục và kiểm tra đĩa để gây thiệt hại về thể chất. Đừng đọc đi đọc lại những thông tin rõ ràng hoặc bị hư hại, vì những thông tin khác có thể làm nó tệ hơn.

## Bản chú giải

| Kì | Nghĩa là trong GW GUI |
|---|---|
| Điều khiển | Hạt Greaseweazle Giao diện phần cứng kết nối qua USB |
| Lái đi | Name |
| Máy | Thực hiện được chọn để thực hiện một thao tác |
| Dòng chảy | Thông tin thời gian đại diện cho sự chuyển tiếp từ tính đọc từ đĩa |
| Ảnh thô | Bắt giữ thông tin đĩa cấp thấp như SCP |
| Ảnh vùng | Một hình ảnh được giải mã được tổ chức thành các lĩnh vực logic |
| Cách mạng | Name |
| Trụ | Một vị trí đầu đường kính; một trụ có thể chứa một đường ray ở mỗi bên |
| Đầu | Name |
| Hồ sơ | Name |
| ROM | Name |
| Lưu trạng thái | Hình chụp trạng thái giả lập đang chạy |
| Vẽ | Hậu phương đồ hoạ dùng để hiển thị kết quả mô phỏng |

## Tham chiếu nhanh

| Nếu anh muốn... | Tới... |
|---|---|
| Bảo tồn đĩa vật lý | **Đọc** |
| Đặt ảnh lại trên đĩa | **Ghi** |
| Tạo ra định dạng ảnh khác | **Chuyển đổi** |
| Kiểm tra dấu vết hoặc tần số dị thường | **Hình ảnh hoá** |
| Duyệt tập tin bên trong ảnh | **Disk Explorer** |
| Kiểm tra giao tiếp điều khiển | **Công cụ > Thông tin về Controller** |
| Đo ổ quay | **Công cụ > Tốc độ** |
| Xem lại lệnh cũ | **Hành động lịch sử** |
| Cấu hình phần cứng | **Tùy chọn > Điều khiển và lái xe** |
| Chọn thực hiện | **Tùy chọn > Máy** |
| Tạo hay chỉnh sửa máy mô phỏng | **Tùy chọn > Mô phỏng** |
| Khởi động một máy lưu | **Mô phỏng** |
