using System;
using System.Collections.Generic;

namespace Fruitful_Gifts.Database;

public partial class ChatLieu
{
    public int MaChatLieu { get; set; }

    public string? TenChatLieu { get; set; }

    public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
