package cn.com.heaton.shiningmask.dao.bean;

/* JADX INFO: loaded from: classes.dex */
public class InputTextRecord {
    private Long id;
    private String textContent;

    public InputTextRecord(Long l, String str) {
        this.id = l;
        this.textContent = str;
    }

    public InputTextRecord(String str) {
        this.textContent = str;
    }

    public InputTextRecord() {
    }

    public Long getId() {
        return this.id;
    }

    public void setId(Long l) {
        this.id = l;
    }

    public String getTextContent() {
        return this.textContent;
    }

    public void setTextContent(String str) {
        this.textContent = str;
    }
}