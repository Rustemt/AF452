using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Collections;
using System.Configuration;

namespace Nop.Web.Framework.AF
{
    public class FilterIPAttribute : AuthorizeAttribute
    {

        #region Allowed
        /// <summary>
        /// Comma seperated string of allowable IPs. Example "10.2.5.41,192.168.0.22"
        /// </summary>
        /// <value></value>
        public string AllowedSingleIPs { get; set; }

        /// <summary>
        /// Comma seperated string of allowable IPs with masks. Example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0"
        /// </summary>
        /// <value>The masked I ps.</value>
        public string AllowedMaskedIPs { get; set; }

        /// <summary>
        /// Gets or sets the configuration key for allowed single IPs
        /// </summary>
        /// <value>The configuration key single I ps.</value>
        public string ConfigurationKeyAllowedSingleIPs { get; set; }

        /// <summary>
        /// Gets or sets the configuration key allowed mmasked IPs
        /// </summary>
        /// <value>The configuration key masked I ps.</value>
        public string ConfigurationKeyAllowedMaskedIPs { get; set; }

        /// <summary>
        /// List of allowed IPs
        /// </summary>
        IPList allowedIPListToCheck = new IPList();
        #endregion

        #region Denied
        /// <summary>
        /// Comma seperated string of denied IPs. Example "10.2.5.41,192.168.0.22"
        /// </summary>
        /// <value></value>
        public string DeniedSingleIPs { get; set; }

        /// <summary>
        /// Comma seperated string of denied IPs with masks. Example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0"
        /// </summary>
        /// <value>The masked I ps.</value>
        public string DeniedMaskedIPs { get; set; }


        /// <summary>
        /// Gets or sets the configuration key for denied single IPs
        /// </summary>
        /// <value>The configuration key single I ps.</value>
        public string ConfigurationKeyDeniedSingleIPs { get; set; }

        /// <summary>
        /// Gets or sets the configuration key for denied masked IPs
        /// </summary>
        /// <value>The configuration key masked I ps.</value>
        public string ConfigurationKeyDeniedMaskedIPs { get; set; }

        /// <summary>
        /// List of denied IPs
        /// </summary>
        IPList deniedIPListToCheck = new IPList();
        #endregion


        /// <summary>
        /// Determines whether access to the core framework is authorized.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>
        /// true if access is authorized; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            string userIpAddress = httpContext.Request.UserHostAddress;

            try
            {
                // Check that the IP is allowed to access
                bool ipAllowed = CheckAllowedIPs(userIpAddress);

                // Check that the IP is not denied to access
                bool ipDenied = CheckDeniedIPs(userIpAddress);

                // Only allowed if allowed and not denied
                bool finallyAllowed = ipAllowed && !ipDenied;

                return finallyAllowed;
            }
            catch (Exception e)
            {
                // Log the exception, probably something wrong with the configuration
            }

            return true; // if there was an exception, then we return true
        }

        /// <summary>
        /// Checks the allowed IPs.
        /// </summary>
        /// <param name="userIpAddress">The user ip address.</param>
        /// <returns></returns>
        private bool CheckAllowedIPs(string userIpAddress)
        {
            // Populate the IPList with the Single IPs
            if (!string.IsNullOrEmpty(AllowedSingleIPs))
            {
                SplitAndAddSingleIPs(AllowedSingleIPs, allowedIPListToCheck);
            }

            // Populate the IPList with the Masked IPs
            if (!string.IsNullOrEmpty(AllowedMaskedIPs))
            {
                SplitAndAddMaskedIPs(AllowedMaskedIPs, allowedIPListToCheck);
            }

            // Check if there are more settings from the configuration (Web.config)
            if (!string.IsNullOrEmpty(ConfigurationKeyAllowedSingleIPs))
            {
                string configurationAllowedAdminSingleIPs = ConfigurationManager.AppSettings[ConfigurationKeyAllowedSingleIPs];
                if (!string.IsNullOrEmpty(configurationAllowedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(configurationAllowedAdminSingleIPs, allowedIPListToCheck);
                }
            }

            if (!string.IsNullOrEmpty(ConfigurationKeyAllowedMaskedIPs))
            {
                string configurationAllowedAdminMaskedIPs = ConfigurationManager.AppSettings[ConfigurationKeyAllowedMaskedIPs];
                if (!string.IsNullOrEmpty(configurationAllowedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(configurationAllowedAdminMaskedIPs, allowedIPListToCheck);
                }
            }

            return allowedIPListToCheck.CheckNumber(userIpAddress);
        }

        /// <summary>
        /// Checks the denied IPs.
        /// </summary>
        /// <param name="userIpAddress">The user ip address.</param>
        /// <returns></returns>
        private bool CheckDeniedIPs(string userIpAddress)
        {
            // Populate the IPList with the Single IPs
            if (!string.IsNullOrEmpty(DeniedSingleIPs))
            {
                SplitAndAddSingleIPs(DeniedSingleIPs, deniedIPListToCheck);
            }

            // Populate the IPList with the Masked IPs
            if (!string.IsNullOrEmpty(DeniedMaskedIPs))
            {
                SplitAndAddMaskedIPs(DeniedMaskedIPs, deniedIPListToCheck);
            }

            // Check if there are more settings from the configuration (Web.config)
            if (!string.IsNullOrEmpty(ConfigurationKeyDeniedSingleIPs))
            {
                string configurationDeniedAdminSingleIPs = ConfigurationManager.AppSettings[ConfigurationKeyDeniedSingleIPs];
                if (!string.IsNullOrEmpty(configurationDeniedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(configurationDeniedAdminSingleIPs, deniedIPListToCheck);
                }
            }

            if (!string.IsNullOrEmpty(ConfigurationKeyDeniedMaskedIPs))
            {
                string configurationDeniedAdminMaskedIPs = ConfigurationManager.AppSettings[ConfigurationKeyDeniedMaskedIPs];
                if (!string.IsNullOrEmpty(configurationDeniedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(configurationDeniedAdminMaskedIPs, deniedIPListToCheck);
                }
            }

            return deniedIPListToCheck.CheckNumber(userIpAddress);
        }

        /// <summary>
        /// Splits the incoming ip string of the format "IP,IP" example "10.2.0.0,10.3.0.0" and adds the result to the IPList
        /// </summary>
        /// <param name="ips">The ips.</param>
        /// <param name="list">The list.</param>
        private void SplitAndAddSingleIPs(string ips, IPList list)
        {
            var splitSingleIPs = ips.Split(',');
            foreach (string ip in splitSingleIPs)
                list.Add(ip);
        }

        /// <summary>
        /// Splits the incoming ip string of the format "IP;MASK,IP;MASK" example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0" and adds the result to the IPList
        /// </summary>
        /// <param name="ips">The ips.</param>
        /// <param name="list">The list.</param>
        private void SplitAndAddMaskedIPs(string ips, IPList list)
        {
            var splitMaskedIPs = ips.Split(',');
            foreach (string maskedIp in splitMaskedIPs)
            {
                var ipAndMask = maskedIp.Split(';');
                list.Add(ipAndMask[0], ipAndMask[1]); // IP;MASK
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }
    }

    /// <summary>
    /// Internal class for storing a range of IP numbers with the same IP mask
    /// </summary>
    internal class IPArrayList {
        private bool isSorted = false;
        private ArrayList ipNumList = new ArrayList();
        private uint ipmask;

        /// <summary>
        /// Constructor that sets the mask for the list
        /// </summary>
        public IPArrayList(uint mask) {
            ipmask = mask;
        }

        /// <summary>
        /// Add a new IP numer (range) to the list
        /// </summary>
        public void Add(uint IPNum) {
            isSorted = false;
            ipNumList.Add(IPNum & ipmask);
        }

        /// <summary>
        /// Checks if an IP number is within the ranges included by the list
        /// </summary>
        public bool Check(uint IPNum) {
            bool found = false;
            if (ipNumList.Count>0) {
                if (!isSorted) {
                    ipNumList.Sort();
                    isSorted=true;
                }
                IPNum = IPNum & ipmask;
                if (ipNumList.BinarySearch(IPNum)>=0) found=true;
            }
            return found;
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        public void Clear() {
            ipNumList.Clear();
            isSorted = false;
        }

        /// <summary>
        /// The ToString is overriden to generate a list of the IP numbers
        /// </summary>
        public override string ToString() {
            StringBuilder buf = new StringBuilder();
            foreach (uint ipnum in ipNumList) {
                if (buf.Length>0) buf.Append("\r\n");
                buf.Append(((int)ipnum & 0xFF000000) >> 24).Append('.');
                buf.Append(((int)ipnum & 0x00FF0000) >> 16).Append('.');
                buf.Append(((int)ipnum & 0x0000FF00) >> 8).Append('.');
                buf.Append(((int)ipnum & 0x000000FF));
            }
            return buf.ToString();
        }

        /// <summary>
        /// The IP mask for this list of IP numbers
        /// </summary>
        public uint Mask {
            get {
                return ipmask;
            }
        }
    }

    /// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class IPList
	{   private ArrayList ipRangeList = new ArrayList();
        private SortedList maskList = new SortedList();
        private ArrayList usedList = new ArrayList();

		public IPList()
		{
            // Initialize IP mask list and create IPArrayList into the ipRangeList
            uint mask = 0x00000000;
            for(int level = 1; level<33; level++) {
                mask = (mask >> 1) | 0x80000000;
                maskList.Add(mask,level);
                ipRangeList.Add(new IPArrayList(mask));
            }
        }

        // Parse a String IP address to a 32 bit unsigned integer
        // We can't use System.Net.IPAddress as it will not parse
        // our masks correctly eg. 255.255.0.0 is pased as 65535 !
        private uint parseIP(string IPNumber) {
            uint res = 0;
            string[] elements = IPNumber.Split(new Char[] {'.'});
            if (elements.Length==4) {
                res = (uint)Convert.ToInt32(elements[0])<<24;
                res += (uint)Convert.ToInt32(elements[1])<<16;
                res += (uint) Convert.ToInt32(elements[2])<<8;
                res += (uint) Convert.ToInt32(elements[3]);
            }
            return res;
        }

        /// <summary>
        /// Add a single IP number to the list as a string, ex. 10.1.1.1
        /// </summary>
        public void Add(string ipNumber) {
            this.Add(parseIP(ipNumber));
        }

        /// <summary>
        /// Add a single IP number to the list as a unsigned integer, ex. 0x0A010101
        /// </summary>
        public void Add(uint ip) {
            ((IPArrayList)ipRangeList[31]).Add(ip);
            if (!usedList.Contains((int)31)) {
                usedList.Add((int)31);
                usedList.Sort();
            }
        }

        /// <summary>
        /// Adds IP numbers using a mask for range where the mask specifies the number of
        /// fixed bits, ex. 172.16.0.0 255.255.0.0 will add 172.16.0.0 - 172.16.255.255
        /// </summary>
        public void Add(string ipNumber, string mask) {
            this.Add(parseIP(ipNumber),parseIP(mask));
        }

        /// <summary>
        /// Adds IP numbers using a mask for range where the mask specifies the number of
        /// fixed bits, ex. 0xAC1000 0xFFFF0000 will add 172.16.0.0 - 172.16.255.255
        /// </summary>
        public void Add(uint ip, uint umask) {
            object Level = maskList[umask];
            if (Level!=null) {
                ip = ip & umask;
                ((IPArrayList)ipRangeList[(int)Level-1]).Add(ip);
                if (!usedList.Contains((int)Level-1)) {
                    usedList.Add((int)Level-1);
                    usedList.Sort();
                }
            }
        }

        /// <summary>
        /// Adds IP numbers using a mask for range where the mask specifies the number of
        /// fixed bits, ex. 192.168.1.0/24 which will add 192.168.1.0 - 192.168.1.255
        /// </summary>
        public void Add(string ipNumber, int maskLevel) {
            this.Add(parseIP(ipNumber),(uint)maskList.GetKey(maskList.IndexOfValue(maskLevel)));
        }

        /// <summary>
        /// Adds IP numbers using a from and to IP number. The method checks the range and
        /// splits it into normal ip/mask blocks.
        /// </summary>
        public void AddRange(string fromIP, string toIP) {
            this.AddRange(parseIP(fromIP),parseIP(toIP));
        }

        /// <summary>
        /// Adds IP numbers using a from and to IP number. The method checks the range and
        /// splits it into normal ip/mask blocks.
        /// </summary>
        public void AddRange(uint fromIP, uint toIP) {
            // If the order is not asending, switch the IP numbers.
            if (fromIP>toIP) {
                uint tempIP = fromIP;
                fromIP=toIP;
                toIP=tempIP;
            }
            if (fromIP==toIP) {
                this.Add(fromIP);
            } else {
                uint diff = toIP-fromIP;
                int diffLevel = 1;
                uint range = 0x80000000;
                if (diff<256) {
                    diffLevel = 24;
                    range = 0x00000100;
                }
                while (range>diff) {
                    range = range>>1;
                    diffLevel++;
                }
                uint mask = (uint)maskList.GetKey(maskList.IndexOfValue(diffLevel));
                uint minIP = fromIP & mask;
                if (minIP<fromIP) minIP+=range;
                if (minIP>fromIP) {
                    this.AddRange(fromIP,minIP-1);
                    fromIP=minIP;
                }
                if (fromIP==toIP) {
                    this.Add(fromIP);
                } else {
                    if ((minIP+(range-1))<=toIP) {
                        this.Add(minIP,mask);
                        fromIP = minIP+range;
                    }
                    if (fromIP==toIP) {
                        this.Add(toIP);
                    } else {
                        if (fromIP<toIP) this.AddRange(fromIP,toIP);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if an IP number is contained in the lists, ex. 10.0.0.1
        /// </summary>
        public bool CheckNumber(string ipNumber) {
            return this.CheckNumber(parseIP(ipNumber));;
        }

        /// <summary>
        /// Checks if an IP number is contained in the lists, ex. 0x0A000001
        /// </summary>
        public bool CheckNumber(uint ip) {
            bool found = false;
            int i=0;
            while (!found && i<usedList.Count) {
                found = ((IPArrayList)ipRangeList[(int)usedList[i]]).Check(ip);
                i++;
            }
            return found;
        }

        /// <summary>
        /// Clears all lists of IP numbers
        /// </summary>
        public void Clear() {
            foreach (int i in usedList) {
                ((IPArrayList)ipRangeList[i]).Clear();
            }
            usedList.Clear();
        }

        /// <summary>
        /// Generates a list of all IP ranges in printable format
        /// </summary>
        public override string ToString() {
            StringBuilder buffer = new StringBuilder();
            foreach (int i in usedList) {
                buffer.Append("\r\nRange with mask of ").Append(i+1).Append("\r\n");
                buffer.Append(((IPArrayList)ipRangeList[i]).ToString());
            }
            return buffer.ToString();
        }


	}


}
