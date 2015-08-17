
namespace TestLibrary.TestUtils.ToxiproxyUtils {
    using System;

    public interface IsToxiProxyTest {
        string ToxiProxyServerName { get; }
        int? ToxiProxyServerManagementPort { get; }
        string RemoteUrl { get; }
    }
}