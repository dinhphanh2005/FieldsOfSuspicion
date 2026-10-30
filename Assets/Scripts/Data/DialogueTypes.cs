using System;
using System.Collections.Generic;
using UnityEngine;

// Biến thể câu: nếu điều kiện đúng thì dùng text này thay cho câu mặc định.
// Có nhiều biến thể thì lấy biến thể ĐẦU TIÊN thỏa điều kiện.
[Serializable]
public class LineVariant
{
    public ShowCondition condition;
    public string param;
    [TextArea(2, 5)] public string text;
}

// Một lựa chọn trong khung thoại: câu của Tomas + câu đáp của khách.
// Cụm từ chứa tin viết dạng {{cụm từ|F02}}.
[Serializable]
public class DialogueLine
{
    [TextArea(1, 3)] public string tomas;
    [TextArea(2, 5)] public string reply;
    public List<LineVariant> replyVariants = new();
}

// Một lượt khách ghé tiệm trong một ngày.
[Serializable]
public class CustomerVisit
{
    public CustomerSO customer;
    [TextArea(2, 4)] public string greeting;
    public List<LineVariant> greetingVariants = new();

    [Tooltip("Số xu trả khi Hỏi chuyện (Sable = 5)")]
    public int askCost;

    public DialogueLine sell;       // Bán hàng
    public DialogueLine discount;   // Bớt giá
    public DialogueLine ask;        // Hỏi chuyện
    public DialogueLine press;      // Hỏi dồn = bấm Hỏi chuyện lần 2 trong cùng lượt khách
    public DialogueLine farewell;   // Tiễn khách
}
