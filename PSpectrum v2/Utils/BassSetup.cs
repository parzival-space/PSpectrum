using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace PSpectrum.Utils
{
    class BassSetup
    {
        public static void Install()
        {
            if (IsInstalled()) return;
            InstallBass();
        }

        private static bool IsInstalled()
        {
            // check if files already exist
            var validationScore = 0;

            if (!File.Exists("bass.dll")) validationScore++;
            if (!File.Exists("basswasapi.dll")) validationScore++;

            // if the validation score is equal to 0, everything is fine
            return validationScore == 0;
        }

        private static void InstallBass()
        {
            // install bass.dll
            using (var writer = File.OpenWrite("bass.dll"))
            {
                writer.Write(
                    Properties.Resources.bass, 
                    0, 
                    Properties.Resources.bass.Length
                );
            }

            // install basswasapi.dll
            using (var writer = File.OpenWrite("basswasapi.dll"))
            {
                writer.Write(
                    Properties.Resources.basswasapi,
                    0,
                    Properties.Resources.basswasapi.Length
                );
            }
        }
    }
}
