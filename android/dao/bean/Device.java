package cn.com.heaton.shiningmask.dao.bean;

/* JADX INFO: loaded from: classes.dex */
public class Device {
    private String deviceName;
    private Long id;
    private boolean isReConnect;
    private String mac;

    public Device(Long l, String str, String str2, boolean z) {
        this.id = l;
        this.deviceName = str;
        this.mac = str2;
        this.isReConnect = z;
    }

    public Device() {
    }

    public Long getId() {
        return this.id;
    }

    public void setId(Long l) {
        this.id = l;
    }

    public String getDeviceName() {
        return this.deviceName;
    }

    public void setDeviceName(String str) {
        this.deviceName = str;
    }

    public String getMac() {
        return this.mac;
    }

    public void setMac(String str) {
        this.mac = str;
    }

    public boolean getIsReConnect() {
        return this.isReConnect;
    }

    public void setIsReConnect(boolean z) {
        this.isReConnect = z;
    }
}