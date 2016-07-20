using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1 {
    class Config {
        public static int ACTION_TYPE_FILL = 12345;
        public static int ACTION_TYPE_CLICK = 54321;

        public String SSID;
        public String[] fingerprints;

        public int[] actionType;
        public String[] actionTag;
        public int[] actionIndex;
        public String[] actionValue;

        public Config() { }

        public bool identify(String documentText) {
            bool result = true;

            WlanClient wlan = new WlanClient();
            Wlan.Dot11Ssid ssid;
            String ssidString = "";
            foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces) {
                ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                ssidString = new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength));
            }

            if (SSID.Equals(ssidString)) {
                foreach (String fingerprint in this.fingerprints) {
                    if (!documentText.Contains(fingerprint)) {
                        result = false;
                    }
                }
            }
            else {
                result = false;
            }

            return result;
        }

        public static Config[] readFromFile() {
            Config[] configs = null;

            String file =
                "SSID-->AA/27,.," +
                "FINGERPRINT--><title>LOGIN PAGE</title>,.," +

                "ACTION_TYPE-->FILL,.," +
                "ACTION_TAG-->input,.," +
                "ACTION_INDEX-->0,.," +
                "ACTION_VALUE-->yohanesmario,.," +

                "ACTION_TYPE-->FILL,.," +
                "ACTION_TAG-->input,.," +
                "ACTION_INDEX-->1,.," +
                "ACTION_VALUE-->whitemouse,.," +

                "ACTION_TYPE-->CLICK,.," +
                "ACTION_TAG-->button,.," +
                "ACTION_INDEX-->0,.," +
                
                "|**|" +
                
                "SSID-->Yohanes Mario Chandra,.," +
                "FINGERPRINT--><title>LOGIN PAGE - TESTING</title>,.," +

                "ACTION_TYPE-->FILL,.," +
                "ACTION_TAG-->input,.," +
                "ACTION_INDEX-->0,.," +
                "ACTION_VALUE-->username,.," +

                "ACTION_TYPE-->FILL,.," +
                "ACTION_TAG-->input,.," +
                "ACTION_INDEX-->1,.," +
                "ACTION_VALUE-->password,.,";

            String[] separator = {"|**|"};
            String[] components = file.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            configs = new Config[components.Length];
            int i = 0;

            foreach (String component in components) {
                String trimedComponent = component.Trim();
                separator[0] = ",.,";
                String[] lines = trimedComponent.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                configs[i] = new Config();

                String[][] commands = new String[lines.Length][];
                int j = 0;

                int fingerprintCounter = 0;
                int actionCounter = 0;
                foreach (String line in lines) {
                    commands[j] = new String[2];
                    String trimedLine = line.Trim();
                    separator[0] = "-->";
                    commands[j] = trimedLine.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    commands[j][0] = commands[j][0].Trim();
                    commands[j][1] = commands[j][1].Trim();
                    if (commands[j][0].Equals("ACTION_TYPE")) {
                        actionCounter++;
                    }
                    else if (commands[j][0].Equals("FINGERPRINT")) {
                        fingerprintCounter++;
                    }
                    j++;
                }

                String[] fingerprints = new String[fingerprintCounter];

                int[] actionType = new int[actionCounter];
                String[] actionTag = new String[actionCounter];
                int[] actionIndex = new int[actionCounter];
                String[] actionValue = new String[actionCounter];

                int fingerprintIdx = 0;
                int actionIdx = 0;
                for (int idx = 0; idx < commands.Length; idx++) {
                    if (commands[idx][0].Equals("SSID")) {
                        configs[i].SSID = commands[idx][1];
                    }
                    else if (commands[idx][0].Equals("FINGERPRINT")) {
                        fingerprints[fingerprintIdx] = commands[idx][1];
                        fingerprintIdx++;
                    }
                    else if (commands[idx][0].Equals("ACTION_TYPE")) {
                        if (commands[idx][1].Equals("FILL")) {
                            actionType[actionIdx] = Config.ACTION_TYPE_FILL;
                            actionTag[actionIdx] = commands[idx + 1][1];
                            actionIndex[actionIdx] = int.Parse(commands[idx + 2][1]);
                            actionValue[actionIdx] = commands[idx + 3][1];
                            idx += 3;
                        }
                        else if (commands[idx][1].Equals("CLICK")) {
                            actionType[actionIdx] = Config.ACTION_TYPE_CLICK;
                            actionTag[actionIdx] = commands[idx + 1][1];
                            actionIndex[actionIdx] = int.Parse(commands[idx + 2][1]);
                            actionValue[actionIdx] = null;
                            idx += 2;
                        }
                        actionIdx++;
                    }
                }

                configs[i].fingerprints = fingerprints;
                configs[i].actionType = actionType;
                configs[i].actionTag = actionTag;
                configs[i].actionIndex = actionIndex;
                configs[i].actionValue = actionValue;

                i++;
            }

            return configs;
        }
    }
}
