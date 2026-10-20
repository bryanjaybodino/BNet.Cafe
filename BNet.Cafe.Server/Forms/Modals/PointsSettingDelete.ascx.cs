using System;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms.Modals
{
    public partial class PointsSettingDelete : UserControl
    {
        protected void LinkButton_ConfirmDelete_Click(object sender, EventArgs e)
        {
            string eventTarget = Request.Form["__EVENTTARGET"] ?? string.Empty;
            if (!eventTarget.Contains(LinkButton_ConfirmDelete.ID))
            {
                return;
            }

            PointsRates pointsRepository = new PointsRates();
            bool isSuccess = pointsRepository.Delete(HiddenField_DeletePointId.Value);

            if (isSuccess)
            {
                AlertService.ShowAlert(this, "Points rule deleted successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(this, "Failed to delete points rule.", "error");
            }
        }
    }
}