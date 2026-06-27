package cn.com.heaton.shiningmask.model.bean;

import java.util.ArrayList;
import java.util.Arrays;

/* JADX INFO: loaded from: classes.dex */
public class DiyData {
    private static final long serialVersionUID = -3139325922167935911L;
    private ArrayList<Integer> colorArray;
    private byte[] data;
    private Long diyId;
    private boolean selectStatus;

    public DiyData(byte[] bArr, ArrayList<Integer> arrayList) {
        this.data = bArr;
        this.colorArray = arrayList;
    }

    public DiyData(Long l, boolean z, byte[] bArr, ArrayList<Integer> arrayList) {
        this.diyId = l;
        this.selectStatus = z;
        this.data = bArr;
        this.colorArray = arrayList;
    }

    public DiyData() {
    }

    public Long getDiyId() {
        return this.diyId;
    }

    public void setDiyId(Long l) {
        this.diyId = l;
    }

    public byte[] getData() {
        return this.data;
    }

    public void setData(byte[] bArr) {
        this.data = bArr;
    }

    public ArrayList<Integer> getColorArray() {
        return this.colorArray;
    }

    public void setColorArray(ArrayList<Integer> arrayList) {
        this.colorArray = arrayList;
    }

    public boolean getSelectStatus() {
        return this.selectStatus;
    }

    public void setSelectStatus(boolean z) {
        this.selectStatus = z;
    }

    public String toString() {
        return "DiyData{diyId=" + this.diyId + ", selectStatus=" + this.selectStatus + ", data=" + Arrays.toString(this.data) + ", colorArray=" + this.colorArray + '}';
    }
}