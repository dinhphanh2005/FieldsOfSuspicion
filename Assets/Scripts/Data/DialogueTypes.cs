using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineVariant
{
    public ShowCondition condition;
    public string param;
    [TextArea(2, 5)] public string text;
}

[Serializable]
public class DialogueLine
{
    [TextArea(1, 3)] public string tomas;
    [TextArea(2, 5)] public string reply;
    public List<LineVariant> replyVariants = new();
}

[Serializable]
public class CustomerVisit
{
    public CustomerSO customer;

    [Tooltip("Ngưỡng đà khách quen cần có để lượt khách phụ này ghé. Khách cốt truyện để 0.")]
    [Min(0)] public int requiredMomentum;

    [Tooltip("Dẫn truyện khi khách bước vào (in nghiêng)")]
    [TextArea(2, 4)] public string entrance;
    public List<LineVariant> entranceVariants = new();

    [TextArea(2, 4)] public string greeting;
    public List<LineVariant> greetingVariants = new();

    [Tooltip("Số xu trả khi Hỏi chuyện (Sable = 5)")]
    public int askCost;

    [Tooltip("Tiền nhận khi Bán hàng / Bớt giá (số tạm, chỉnh khi playtest)")]
    public int sellPrice = 10;
    public int discountPrice = 6;

    [Tooltip("Số bó rau khách mua ở giá thường. 0 = khách không mua; -1 = mua hết kho.")]
    public int sellQuantity = 1;

    [Tooltip("Số bó rau khách lấy khi bớt giá. 0 = chỉ trao đổi lời/tin, không xuất rau.")]
    public int discountQuantity = 1;

    [Tooltip("Nhãn thay cho lựa chọn Bớt giá nếu đây là một cuộc trao đổi đặc biệt.")]
    public string discountActionLabel;

    [Tooltip("Tắt nếu khách từ chối giảm giá và vẫn trả giá thường.")]
    public bool discountAccepted = true;

    [Tooltip("Điểm nghi ngờ khi Hỏi dồn (Thấp = 1, Emel N7 = 2)")]
    public int pressSuspicion = 1;

    public DialogueLine sell;
    public DialogueLine discount;
    public DialogueLine ask;
    public DialogueLine press;
    public DialogueLine farewell;

    [Tooltip("Dẫn truyện khi khách rời đi (in nghiêng)")]
    [TextArea(2, 4)] public string exit;
    public List<LineVariant> exitVariants = new();
}
