using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Firefox;

namespace QARobot
{
    class CookieProfile
    {
        public static FirefoxProfile CookieProfiles(string cookieProfile)
        {
            var profile = new FirefoxProfile();
            if (cookieProfile != String.Empty)
            {
                FirefoxProfileManager myProfile = new FirefoxProfileManager();
                profile = myProfile.GetProfile(cookieProfile);
                
            }
            return profile;
        }
    }
}
