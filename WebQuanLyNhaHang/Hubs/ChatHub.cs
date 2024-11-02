using Microsoft.AspNetCore.SignalR;

namespace WebQuanLyNhaHang.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            // Gửi tin nhắn đến tất cả các client đã kết nối
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
        // Phương thức gọi khi có thay đổi trong database (chẳng hạn khi thêm mới)
        public async Task NotifyDatabaseChange()
        {
            // Phát sự kiện thông báo cho tất cả các client
            await Clients.All.SendAsync("DatabaseUpdated");
        }
            public async Task NotifyProductDeleted(int productId)
            {
            await Clients.All.SendAsync("ProductDeleted", productId);
            }

        public async Task NotifyOderSuccess()
        {
            Console.WriteLine("Phát sự kiện OderSuccess");
            await Clients.All.SendAsync("OderSuccess");
        }
    }
}
