using Protractor;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.UI.Model
{
    public class AdminRushRequest : PageObjectBase
    {
        public AdminRushRequest(NgWebDriver ngDriver)
            : base(ngDriver) { }

        public TextArea DenialNoteTextArea
        {
            get
            {
                return GetObject<TextArea>("denialNote");
            }
        }

        public Button DenyRequestButton
        {
            get
            {
                return GetObject<Button>("denyButton");
            }
        }

    }
}
