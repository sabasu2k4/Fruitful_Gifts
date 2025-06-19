using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private static ConcurrentDictionary<string, string> _admins = new();
    private static ConcurrentDictionary<string, string> _clients = new();
    private static ConcurrentDictionary<string, string> _staffs = new(); // Thêm nhân viên

    public override Task OnConnectedAsync()
    {
        var role = Context.GetHttpContext()?.Request.Query["role"].ToString();
        var name = Context.GetHttpContext().Request.Query["name"];
        if (role == "admin")
        {
            _admins[Context.ConnectionId] = Context.ConnectionId;
        }
        else if (role == "nhanvien")
        {
            _staffs[Context.ConnectionId] = Context.ConnectionId;
        }
        else // Mặc định là khách
        {
            //_clients[Context.ConnectionId] = Context.ConnectionId;
            if (!string.IsNullOrEmpty(role) && role == "khach")
            {
                _clients[Context.ConnectionId] = !string.IsNullOrEmpty(name) ? name : "Khách" + Context.ConnectionId;
            }
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(System.Exception exception)
    {
        _admins.TryRemove(Context.ConnectionId, out _);
        _clients.TryRemove(Context.ConnectionId, out _);
        _staffs.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    // Khách gửi tin cho admin và/hoặc nhân viên
    public async Task SendFromClient(string message)
    {
        var connectionId = Context.ConnectionId;
        var name = _clients.ContainsKey(connectionId) ? _clients[connectionId] : "Khách" + connectionId;

        // Gửi cho tất cả admin
        foreach (var adminId in _admins.Values)
        {
            await Clients.Client(adminId).SendAsync("ReceiveFromClient", connectionId, name, message);
        }

        // Gửi cho tất cả nhân viên
        foreach (var staffId in _staffs.Values)
        {
            await Clients.Client(staffId).SendAsync("ReceiveFromClient", connectionId, name, message);
        }
    }

    // Admin hoặc nhân viên gửi tin lại cho khách
    public async Task SendFromAdminOrStaff(string toClientId, string message)
    {
        await Clients.Client(toClientId).SendAsync("ReceiveFromAdmin", message);
    }
}
