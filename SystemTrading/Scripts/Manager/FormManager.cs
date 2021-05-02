using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemTrading.Forms;

public enum eForm
{
    Indicator,
    APIController,
    UserInfoForm,
    OrderStock,
    ProgramSetting,
    AutoTrading,
    SearchStock,
    ToastMessage,
    ProgramOrder,
    RecodeAccountInfo,
}

public class FormManager
{
    public static Form MainForm { get; set; } = null;

    public static Form GetForm(eForm formType)
    {
        foreach (Form openForm in Application.OpenForms)
        {
            if (openForm.Name == formType.ToString())
            {
                return openForm;
            }
        }

        // 열려있지 않은 경우
        return null;
    }

    public static Form OpenForm(eForm formType, Control control = null)
    {
        return OpenForm<Form>(formType, control);
    }

    public static T OpenForm<T>(eForm formType, Control control = null) where T : Form
    {
        foreach (Form openForm in Application.OpenForms)
        {
            if (openForm.Name == formType.ToString())
            {
                // 숨겨져있으면 노출
                if (!openForm.Visible)
                    openForm.Show();

                // 폼을 최소화시켜 하단에 내려놓았는지 검사
                if (openForm.WindowState == FormWindowState.Minimized)
                    openForm.WindowState = FormWindowState.Normal;

                // 활성화
                openForm.Activate();
                return (T)openForm;
            }
        }

        Form form = null;
        switch (formType)
        {
            case eForm.UserInfoForm:
                form = new UserInfoForm();
                break;
            case eForm.OrderStock:
                form = new OrderStockForm();
                break;
            case eForm.ProgramSetting:
                form = new ProgramSetting();
                break;
            case eForm.AutoTrading:
                form = new ConditionTrading();
                break;
            case eForm.SearchStock:
                form = new SearchStock();
                break;
            case eForm.ToastMessage:
                form = new ToastMessage();
                break;
            case eForm.ProgramOrder:
                form = new ProgramOrder();
                break;
            case eForm.RecodeAccountInfo:
                form = new RecodeAccountInfo();
                break;
        }

        if (form != null)
        {
            if (control != null)
            {
                form.TopLevel = false;
                control.Controls.Add(form);
                form.Parent = control;
            }
            form.Show();
            return (T)form;
        }
        return null;
    }
}
