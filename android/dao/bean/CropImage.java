package cn.com.heaton.shiningmask.dao.bean;

import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class CropImage {
    private Long id;
    private byte[] imageData;
    private int imageIndex;
    private String imageUrl;
    private int index;
    private boolean selectStatus;
    private byte[] time;
    private int timeInt;

    public CropImage(Long l, String str, boolean z, byte[] bArr, byte[] bArr2, int i, int i2, int i3) {
        this.id = l;
        this.imageUrl = str;
        this.selectStatus = z;
        this.imageData = bArr;
        this.time = bArr2;
        this.index = i;
        this.timeInt = i2;
        this.imageIndex = i3;
    }

    public CropImage(String str, byte[] bArr, int i) {
        this.index = 0;
        this.imageUrl = str;
        this.imageData = bArr;
        this.imageIndex = i;
    }

    public CropImage() {
        this.index = 0;
    }

    public Long getId() {
        return this.id;
    }

    public void setId(Long l) {
        this.id = l;
    }

    public String getImageUrl() {
        return this.imageUrl;
    }

    public void setImageUrl(String str) {
        this.imageUrl = str;
    }

    public byte[] getImageData() {
        return this.imageData;
    }

    public void setImageData(byte[] bArr) {
        this.imageData = bArr;
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

    public byte[] getTime() {
        return this.time;
    }

    public void setTime(byte[] bArr) {
        this.time = bArr;
    }

    public int getTimeInt() {
        return this.timeInt;
    }

    public void setTimeInt(int i) {
        this.timeInt = i;
    }

    public int getImageIndex() {
        return this.imageIndex;
    }

    public void setImageIndex(int i) {
        this.imageIndex = i;
    }

    public String toString() {
        return "CropImage{id=" + this.id + ", imageUrl='" + this.imageUrl + "', selectStatus=" + this.selectStatus + ", imageData=" + Arrays.toString(this.imageData) + ", time=" + Arrays.toString(this.time) + ", index=" + this.index + ", timeInt=" + this.timeInt + ", imageIndex=" + this.imageIndex + '}';
    }
}