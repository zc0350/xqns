using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Configuration;
using System.Collections;
using System.Configuration.Install;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;



namespace xqns
{
    public partial class Form1 : Form
    {
        const string website = "https://www.xqns.com";
        const string loginurl = "https://login.xqns.com/login.php";
        const string sname = "xqnsvc";
        const string fname = "\\xqns.xml";
        string url = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void 退出程序QToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(website);
        }

        private void 注册账号RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(website + "/reg");
        }

        private void 找回密码FToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(website + "/findpass");
        }

        private void 官方网站WToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(website);
        }

        private void 使用帮助HToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(website + "/help");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readxml();
            if (textBox1.Text!="" && textBox2.Text != "" && textBox3.Text != "")
            {
                timer1.Enabled = true;
                timer1.Interval = 10000;
                label5.Text = "10秒后连接";
            }else
            {
             label5.Text = "";
            }
            //检查服务是否安装

            if (ServiceExist())
            {
                //如果服务已存在//禁用安装
                button3.Enabled = false;
                安装服务IToolStripMenuItem.Enabled = false;
                button4.Enabled = true;
                卸载服务RToolStripMenuItem.Enabled = true;

            }
            else
            {
                button3.Enabled = true;
                安装服务IToolStripMenuItem.Enabled = true;
                button4.Enabled = false;
                卸载服务RToolStripMenuItem.Enabled = false;

            }
        }
        private void readxml()
        {
            string text = Application.StartupPath + fname;
            if (File.Exists(text))
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.Load(text);
                    XmlNodeList node = xml.SelectSingleNode("PostData").ChildNodes;
                    foreach (XmlNode list in node)
                    {
                        if (list.Name == "User") textBox1.Text = list.InnerText.Trim();
                        if (list.Name == "Pass") textBox2.Text = DeDES(list.InnerText.Trim(),website); 
                        if (list.Name == "Key") textBox3.Text = list.InnerText.Trim();
                    }         
                }
                catch  {                }
            }                         
        }

        private void UnInstall()
        {
            if (!ServiceExist()) {
                MessageBox.Show("系统不存在此服务，不需要卸载！");
                return;
            }
            label5.Text = "正在卸载";
            try {
                ServiceStop();
                string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string serviceFileName = location.Substring(0, location.LastIndexOf('\\') + 1) + "xqnsvc.exe";
                AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
                AssemblyInstaller1.UseNewContext = true;
                AssemblyInstaller1.Path = serviceFileName;
                AssemblyInstaller1.Uninstall(null);
                AssemblyInstaller1.Dispose();
                label5.Text = "卸载完成";
            }
                catch (Exception ex)
                {
                MessageBox.Show(ex.Message);
                label5.Text = "卸载异常";
            }        
        }

        private void InstallService()
        {
            if (ServiceExist())
            {
                MessageBox.Show("服务已经安装,如需要重新安装请先卸载!");
                return;
            }
            label5.Text = "正在安装";
            try
            {
                string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string serviceFileName = location.Substring(0, location.LastIndexOf('\\') + 1) + "xqnsvc.exe";

                AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
                AssemblyInstaller1.UseNewContext = true;
                AssemblyInstaller1.Path = serviceFileName;
                AssemblyInstaller1.Install(null);
                AssemblyInstaller1.Commit(null);
                AssemblyInstaller1.Dispose();
                label5.Text = "安装完成";
                ServiceStart();
                label5.Text = "服务已启动";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                label5.Text = "安装异常";
            }
        }


        public static bool ServiceStart()
        {
            try
            {
                ServiceController service = new ServiceController(sname);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    return true;
                }
                else
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);               
                    service.Start();             
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool ServiceStop()
        {
            try
            {
                ServiceController service = new ServiceController(sname);
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    return true;
                }
                else
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(1000 * 10);
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
            }
            catch
            {
               return false;
            }
            return true;
        }


        public static bool ServiceIsRunning()
        {
            ServiceController service = new ServiceController(sname);
            if (service.Status == ServiceControllerStatus.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static bool ServiceExist()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController s in services)
                {
                    if (s.ServiceName.ToLower() == sname.ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UnInstall();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            InstallService();
        }

        private void 安装服务IToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallService();
        }

        private void 卸载服务RToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnInstall();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            login();
        }


        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public static string EnDES(string encryptString, string encryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return "";
            }
        }       

        public static string DeDES(string decryptString, string decryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "") {
                MessageBox.Show("请输入域名作为账号！");
                textBox1.Focus();
                return;
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("请输入二级域名密码！");
                textBox2.Focus();
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("请输入顶级域名密钥！");
                textBox3.Focus();
                return;
            }

            try {
                //写入xml
                var xmlDoc = new XmlDocument();
                //Create the xml declaration first 
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));
                //Create the root node and append into doc 
                var el = xmlDoc.CreateElement("PostData");
                XmlAttribute attrID = xmlDoc.CreateAttribute("website");
                attrID.Value = "https://www.xqns.com";
                el.Attributes.Append(attrID);
                XmlAttribute attrName = xmlDoc.CreateAttribute("name");
                attrName.Value = "xqns";
                el.Attributes.Append(attrName);
                xmlDoc.AppendChild(el);
                XmlElement elementName = xmlDoc.CreateElement("Type");
                elementName.InnerText = "domain";
                el.AppendChild(elementName);
                XmlElement elementGender = xmlDoc.CreateElement("User");
                elementGender.InnerText = textBox1.Text.Trim();
                el.AppendChild(elementGender);
                XmlElement elementPass = xmlDoc.CreateElement("Pass");
                elementPass.InnerText = EnDES(textBox2.Text.Trim(), website);
                el.AppendChild(elementPass);            
                XmlElement elementKey = xmlDoc.CreateElement("Key");
                elementKey.InnerText = textBox3.Text.Trim();
                el.AppendChild(elementKey);
                XmlElement elementVer = xmlDoc.CreateElement("Ver");
                elementVer.InnerText = "V3.0";
                el.AppendChild(elementVer);
              xmlDoc.Save("xqns.xml");              
            }
            catch { MessageBox.Show("配置文件写入失败！"); }

            login();

        }
        
        private void login()
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
            {
            timer1.Enabled = false;
            return;
            }
            url = loginurl + "?type=domain&user=" + textBox1.Text.Trim() + "&c=md5&cz=login&ver=V3.0&pass=" + md5(textBox2.Text.Trim() + textBox3.Text.Trim());

            label5.Text = "正在连接";

            ////设置计时器，不管结果如何，计时器启用，并且重连时间为30秒
            timer1.Enabled = true;
            timer1.Interval = 30000;

            try { 
            System.Net.HttpWebResponse res = HttpHelper.CreateGetHttpResponse(url, 5000, null, null);
            if (res == null)
            {
                label5.Text = "网络错误";

                    //    MessageBox.Show("IP更新出错，连接超时，等待下次尝试");
                    return;
            }
            else
            {
                    string mes = HttpHelper.GetResponseString(res);
             
                    //分割//
                    try {
                        string[] sArray= mes.Split('@');
                        string stat = sArray[0].Trim();
                        if (stat == "succ")
                            {
                            label5.Text = "连接成功";
                            timer1.Enabled = true;    //激活计时器（300秒 后重连）
                            timer1.Interval = 300000;                        
                            label8.Text = sArray[1].Trim();
                            button1.Enabled = false;  //成功登陆时禁用，时间到时自动重连
                        }
                        else { 
                            if (stat == "E0")
                            {
                                label5.Text = "域名错误";
                            }
                            if (stat == "E1")
                            {
                                label5.Text = "密码错误";
                            }                          
                            label8.Text = "";
                            ///如果连接不成功，计时器关闭..账号错误，密码错误重连没什么意义
                            timer1.Enabled = false;
                        }                                   
                     }
                    catch
                    {
                        label5.Text = "返回异常";
                    } 
                }
            }
            catch
            {
                label5.Text = "网络错误";
            //    MessageBox.Show("IP更新出错，网络错误");
            }

        }
        private static string md5(string a)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(a, "MD5").ToLower();
        }



        public class HttpHelper
        {
            /// <summary>  
            /// 创建GET方式的HTTP请求  
            /// </summary>  
            public static HttpWebResponse CreateGetHttpResponse(string url, int timeout, string userAgent, CookieCollection cookies)
            {
                HttpWebRequest request = null;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                }
                request.Method = "GET";

                //设置代理UserAgent和超时
                //request.UserAgent = userAgent;
                //request.Timeout = timeout;
                if (cookies != null)
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(cookies);
                }
                return request.GetResponse() as HttpWebResponse;
            }

            /// <summary>  
            /// 创建POST方式的HTTP请求  
            /// </summary>  
            public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int timeout, string userAgent, CookieCollection cookies)
            {
                HttpWebRequest request = null;
                //如果是发送HTTPS请求  
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request = WebRequest.Create(url) as HttpWebRequest;
                    //request.ProtocolVersion = HttpVersion.Version10;
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                }
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                //设置代理UserAgent和超时
                //request.UserAgent = userAgent;
                //request.Timeout = timeout; 

                if (cookies != null)
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(cookies);
                }
                //发送POST数据  
                if (!(parameters == null || parameters.Count == 0))
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (string key in parameters.Keys)
                    {
                        if (i > 0)
                        {
                            buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                        }
                        else
                        {
                            buffer.AppendFormat("{0}={1}", key, parameters[key]);
                            i++;
                        }
                    }
                    byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                string[] values = request.Headers.GetValues("Content-Type");
                return request.GetResponse() as HttpWebResponse;
            }

            /// <summary>
            /// 获取请求的数据
            /// </summary>
            public static string GetResponseString(HttpWebResponse webresponse)
            {
                using (Stream s = webresponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(s, Encoding.UTF8);
                    return reader.ReadToEnd();

                }
            }

            /// <summary>
            /// 验证证书
            /// </summary>
            private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            {
                if (errors == SslPolicyErrors.None)
                    return true;
                return false;
            }
        }

    }
}

