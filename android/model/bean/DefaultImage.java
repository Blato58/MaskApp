package cn.com.heaton.shiningmask.model.bean;

/* JADX INFO: loaded from: classes.dex */
public class DefaultImage {
    private int id;
    private int imgRes;
    private int index;
    private boolean selectStatus;

    public int getImgRes() {
        return this.imgRes;
    }

    public void setImgRes(int i) {
        this.imgRes = i;
    }

    public DefaultImage(int i, int i2, boolean z, int i3) {
        this.id = i;
        this.imgRes = i2;
        this.selectStatus = z;
        this.index = i3;
    }

    public DefaultImage(int i) {
        this.imgRes = i;
    }

    public DefaultImage() {
    }

    public int getId() {
        return this.id;
    }

    public void setId(int i) {
        this.id = i;
    }

    public String toString() {
        return "CropImage{id=" + this.id + ", imageUrl='" + this.imgRes + "', selectStatus=" + this.selectStatus + ", index=" + this.index + '}';
    }

    public boolean getSelectStatus() {
        return this.selectStatus;
    }

    public void setSelectStatus(boolean z) {
        this.selectStatus = z;
    }

    public int getIndex() {
        return this.index;
    }

    public void setIndex(int i) {
        this.index = i;
    }
}