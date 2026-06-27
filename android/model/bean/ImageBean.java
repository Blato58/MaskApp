package cn.com.heaton.shiningmask.model.bean;

/* JADX INFO: loaded from: classes.dex */
public class ImageBean {
    private int imgDesc;
    private int imgRes;
    private int imgResSmall;

    public ImageBean(int i, int i2, int i3) {
        this.imgRes = i;
        this.imgResSmall = i2;
        this.imgDesc = i3;
    }

    public ImageBean(int i, int i2) {
        this.imgRes = i;
        this.imgDesc = i2;
    }

    public int getImgRes() {
        return this.imgRes;
    }

    public void setImgRes(int i) {
        this.imgRes = i;
    }

    public int getImgDesc() {
        return this.imgDesc;
    }

    public void setImgDesc(int i) {
        this.imgDesc = i;
    }

    public int getImgResSmall() {
        return this.imgResSmall;
    }

    public void setImgResSmall(int i) {
        this.imgResSmall = i;
    }
}