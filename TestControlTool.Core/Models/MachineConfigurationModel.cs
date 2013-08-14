using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using TestControlTool.Core.Implementations;

namespace TestControlTool.Core.Models
{
    [DataContract(Name = "MachineConfigurationModel")]
    public class MachineConfigurationModel
    {
        public static string[] HiddenProperties = new[] {"Id", "OwnerUserName", "TimeZoneName"};

        public MachineConfigurationModel()
        {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string OwnerUserName { get; set; }

        [DataMember]
        [Required]
        public string ComputerName { get; set; }

        [Display(Name = "AutoLogon UserName")]
        [DataMember]
        [Required]
        public string AutoLogonUserName { get; set; }

        [Display(Name = "AutoLogon Password")]
        [DataMember]
        [Required]
        [DataType(DataType.Password)]
        public string AutoLogonPassword { get; set; }

        [DataMember]
        [Required]
        public VMServerType MachineType { get; set; }

        [DataMember]
        [Required]
        [Help(Title = "Share Folder", Message = @"Local path to the machine's share folder. For example, 'C:\Share'")]
        public string SharedFolderPath { get; set; }

        [Display(Name = "IP Address")]
        [DataMember]
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "Address doen't match IPv4 standart")]
        public string IPAddress { get; set; }

        [DataMember]
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "SubnetMask doen't match IPv4 standart")]
        public string SubnetMask { get; set; }

        [DataMember]
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "Default Gateway doen't match IPv4 standart")]
        public string DefaultGateway { get; set; }

        [DataMember]
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "DNS doen't match IPv4 standart")]
        public string Dns1 { get; set; }

        [DataMember]
        [Required]
        [RegularExpression(@"^(([01]?\d\d?|2[0-4]\d|25[0-5])\.){3}([01]?\d\d?|25[0-5]|2[0-4]\d)$", ErrorMessage = "DNS doen't match IPv4 standart")]
        public string Dns2 { get; set; }

        [Display(Name = "TimeZone Name")]
        [DataMember]
        [Required]
        public string TimeZoneName { get; set; }
    }
}