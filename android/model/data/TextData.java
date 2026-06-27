package cn.com.heaton.shiningmask.model.data;

import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class TextData {
    private int curIndex;
    private List<byte[]> dataList;

    public TextData(int i, List<byte[]> list) {
        this.curIndex = i;
        this.dataList = list;
    }

    public int getCurIndex() {
        return this.curIndex;
    }

    public void setCurIndex(int i) {
        this.curIndex = i;
    }

    public List<byte[]> getDataList() {
        return this.dataList;
    }

    public void setDataList(List<byte[]> list) {
        this.dataList = list;
    }

    public String toString() {
        return "TextData{curIndex=" + this.curIndex + ", dataList=" + this.dataList + '}';
    }
}