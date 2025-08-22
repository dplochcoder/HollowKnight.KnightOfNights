using KnightOfNights.Scripts.SharedLib;

namespace KnightOfNights.Scripts
{
    static class DeactivatorExtensions
    {
        public static bool Deactivate(this Deactivator self)
        {
            if (self.gameObject.activeSelf)
            {
                self.gameObject.SetActive(false);
                UnityEditorShims.MarkDirty(self.gameObject);
                return true;
            }

            return false;
        }
    }
}
