using UnityEngine.UI;

public class LayoutSafeImage : Image
{
    public override int layoutPriority => -1;
}
