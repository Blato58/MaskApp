package cn.com.heaton.shiningmask.model.bean;

import java.util.ArrayList;

/* JADX INFO: loaded from: classes.dex */
public class HistoryData {
    private ArrayList<Integer> colorList;
    private byte[] data;
    private Long historyId;

    public HistoryData(Long l, byte[] bArr, ArrayList<Integer> arrayList) {
        this.historyId = l;
        this.data = bArr;
        this.colorList = arrayList;
    }

    public HistoryData() {
    }

    public Long getHistoryId() {
        return this.historyId;
    }

    public void setHistoryId(Long l) {
        this.historyId = l;
    }

    public byte[] getData() {
        return this.data;
    }

    public void setData(byte[] bArr) {
        this.data = bArr;
    }

    public ArrayList<Integer> getColorList() {
        return this.colorList;
    }

    public int[] getColorArray() {
        return convertArray(this.colorList);
    }

    public void setColorList(ArrayList<Integer> arrayList) {
        this.colorList = arrayList;
    }

    public void setColorList(int[] iArr) {
        this.colorList = convertList(iArr);
    }

    public int[] convertArray(ArrayList<Integer> arrayList) {
        if (arrayList == null) {
            return null;
        }
        int[] iArr = new int[arrayList.size()];
        for (int i = 0; i < arrayList.size(); i++) {
            iArr[i] = arrayList.get(i).intValue();
        }
        return iArr;
    }

    public ArrayList<Integer> convertList(int[] iArr) {
        if (iArr == null) {
            return null;
        }
        ArrayList<Integer> arrayList = new ArrayList<>();
        for (int i : iArr) {
            arrayList.add(Integer.valueOf(i));
        }
        return arrayList;
    }
}