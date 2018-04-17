using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polossk.Universal.Global
{
    /// <summary>
    /// 登陆与注册的标准反馈消息
    /// </summary>
    public class VerMessage
    {
        public static string PUBLIC_VERIFICATION        = "~publish-server~";
        public static string LOGIN_FAILED_NO_SUCH_USER  = "LoginFailedNoSuchUser";
        public static string LOGIN_FAILED_WRONG_PW      = "LoginFailedWrongPassword";
        public static string LOGIN_SUCCESS              = "LoginSuccess";
        public static string REG_FAILED_NAME_CONFLICT   = "RegFailedNameConflict";
        public static string REG_FAILED_OTHER_PROBLEM   = "RegFailedOtherProblem";
        public static string REG_SUCCESS                = "RegSuccess";
        public static string LOGOFF_SUCCESS             = "LogoffSuccess";
        public static string LOGOFF_FAILED_NO_SUCH_USER = "LogoffFailedNoSuchUser";
        public static string LOGOFF_FAILED_NOT_LOGIN    = "LogoffFailedNotLogin";
        public static string LOGOFF_FAILED              = "LogoffFailed";
        public static string DEFAULT_RESPONSE           = "NOP";
        public static string CHANGE_SUCCESS             = "ChangeSuccess";
        public static string CHANGE_FAILED              = "ChangeFailed";
    }
}
