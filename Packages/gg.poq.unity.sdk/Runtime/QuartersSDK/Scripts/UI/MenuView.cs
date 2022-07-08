namespace QuartersSDK.UI {
    public class MenuView : UIView {
        public UISegue SegueToAuthorizeView;


        public override void ViewWillAppear(UIView sourceView = null) {
            base.ViewWillAppear(sourceView);

            QuartersController.OnSignOut += OnSignOut;
        }

        public override void ViewWillDissappear() {
            base.ViewWillDissappear();

            QuartersController.OnSignOut -= OnSignOut;
        }


        private void OnSignOut() {
            SegueToAuthorizeView.Perform();
        }
    }
}