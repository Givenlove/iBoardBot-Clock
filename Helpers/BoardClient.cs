using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using CsQuery;

namespace iBoardBot.Helpers {
    public class BoardClient {
        private readonly Uri _baseUrl;
        private readonly string _appid;

        public BoardClient(Uri baseUrl, string appid) {
            _baseUrl = baseUrl;
            _appid = appid;
        }

        public void Write(string message) {
            using (var client = new WebClient()) {
                client.BaseAddress = _baseUrl.ToString();

                string workItemId;
                string isLastWorkItem;
                if (TrySendTextToQueue(message, out workItemId, out isLastWorkItem)) {
                    client.UploadValues("send.php", new NameValueCollection {
                        {"APPID", _appid},
                        {"WORK", workItemId},
                        {"LAST", isLastWorkItem}
                    });
                }
            }
        }

        public void Draw(Bitmap bmp) {
            using (var client = new WebClient()) {
                client.BaseAddress = _baseUrl.ToString();

                string workItemId;
                string isLastWorkItem;
                if (TrySendImageToQueue(bmp, out workItemId, out isLastWorkItem)) {
                    client.UploadValues("send.php", new NameValueCollection {
                        {"APPID", _appid},
                        {"WORK", workItemId},
                        {"LAST", isLastWorkItem}
                    });
                }
            }
        }

        public void Clear(int x1 = 0, int y1 = 0, int x2 = 358, int y2 = 105) {
            using (var client = new WebClient()) {
                client.BaseAddress = _baseUrl.ToString();
                client.UploadValues("pErase.php", new NameValueCollection {
                    {"APPID", _appid},
                    {"X1", x1.ToString()},
                    {"Y1", y1.ToString()},
                    {"X2", x2.ToString()},
                    {"Y2", y2.ToString()},
                });
            }
        }

        private bool TrySendTextToQueue(string message, out string workItemId, out string isLastWorkItem) {
            try {
                using (var client = new WebClient()) {
                    client.BaseAddress = _baseUrl.ToString();

                    var response = client.UploadValues("pText.php", new NameValueCollection {
                        {"APPID", _appid},
                        {"TEXT", message}
                    });

                    var result = Encoding.UTF8.GetString(response);

                    GetWorkItemFromDom(out workItemId, out isLastWorkItem, result);

                    return true;
                }
            }
            catch (WebException) {
                workItemId = null;
                isLastWorkItem = null;
                return false;
            }
        }

        private static void GetWorkItemFromDom(out string workItemId, out string isLastWorkItem, string result) {
            var dom = CQ.CreateDocument(result);

            //Get Queue Id
            workItemId = dom["input[name='WORK']"].Val();

            //Get IsLast value (not sure if needed)
            isLastWorkItem = dom["input[name='LAST']"].Val();
        }

        private bool TrySendImageToQueue(Image img, out string workItemId, out string isLastWorkItem) {
            try {
                var url = _baseUrl + "pImage.php";
                var bytes = BitmapToArray(img);
                var result = UploadHelper.HttpUploadFile(url, bytes, "imgfile", "image/png", new NameValueCollection {{"APPID", _appid}});
                var dom = CQ.CreateDocument(result);

                var svgFileName = dom["input[name='SVGLOCALFILE']"].Val();
                var gfile = dom["input[name='GFILE']"].Val();

                using (var client = new WebClient()) {
                    client.BaseAddress = _baseUrl.ToString();

                    var parameters = new NameValueCollection {
                        {"APPID", _appid},
                        {"SVGLOCALFILE", svgFileName},
                        {"GFILE", gfile},
                        {"param1", "0"}, //0 = offset x (CM)
                        {"param2", "0"}, //0 = offset y (CM)
                        {"param3", "100"} //100 = scale
                    };

                    var response = client.UploadValues("pSVG.php", parameters);

                    GetWorkItemFromDom(out workItemId, out isLastWorkItem, Encoding.UTF8.GetString(response));
                }

                return true;
            }
            catch (Exception) {
                workItemId = String.Empty;
                isLastWorkItem = String.Empty;
                return false;
            }
        }

        private static Byte[] BitmapToArray(Image bitmap) {
            using (var stream = new MemoryStream()) {
                bitmap.Save(stream, ImageFormat.Bmp);
                return stream.ToArray();
            }
        }
    }
}