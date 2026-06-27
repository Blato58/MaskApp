package cn.com.heaton.shiningmask.base.app.crash;

/* JADX INFO: loaded from: classes.dex */
public class CrashInfo {
    private String appChannel;
    private String appPackage;
    private String appVersionCode;
    private String appVersionName;
    private String exceptionInfo;
    private String phoneBrands;
    private String phoneModel;
    private String phoneSystem;
    private String phoneSystemVersion;

    public String getAppPackage() {
        return this.appPackage;
    }

    public void setAppPackage(String str) {
        this.appPackage = str;
    }

    public String getAppChannel() {
        return this.appChannel;
    }

    public void setAppChannel(String str) {
        this.appChannel = str;
    }

    public String getPhoneSystem() {
        return this.phoneSystem;
    }

    public void setPhoneSystem(String str) {
        this.phoneSystem = str;
    }

    public String getPhoneBrands() {
        return this.phoneBrands;
    }

    public void setPhoneBrands(String str) {
        this.phoneBrands = str;
    }

    public String getPhoneModel() {
        return this.phoneModel;
    }

    public void setPhoneModel(String str) {
        this.phoneModel = str;
    }

    public String getPhoneSystemVersion() {
        return this.phoneSystemVersion;
    }

    public void setPhoneSystemVersion(String str) {
        this.phoneSystemVersion = str;
    }

    public String getAppVersionName() {
        return this.appVersionName;
    }

    public void setAppVersionName(String str) {
        this.appVersionName = str;
    }

    public String getAppVersionCode() {
        return this.appVersionCode;
    }

    public void setAppVersionCode(String str) {
        this.appVersionCode = str;
    }

    public String getExceptionInfo() {
        return this.exceptionInfo;
    }

    public void setExceptionInfo(String str) {
        this.exceptionInfo = str;
    }

    public String toString() {
        return "CrashInfo{appPackage='" + this.appPackage + "', appChannel='" + this.appChannel + "', phoneSystem='" + this.phoneSystem + "', phoneBrands='" + this.phoneBrands + "', phoneModel='" + this.phoneModel + "', phoneSystemVersion='" + this.phoneSystemVersion + "', appVersionName='" + this.appVersionName + "', appVersionCode='" + this.appVersionCode + "', exceptionInfo='" + this.exceptionInfo + "'}";
    }
}