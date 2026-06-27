package cn.com.heaton.shiningmask.model.bean;

import android.widget.ImageView;

/* JADX INFO: loaded from: classes.dex */
public class RhythmImage {
    private int imageRes;
    private ImageView imageView;
    private boolean isShowImage;

    public RhythmImage() {
    }

    public RhythmImage(int i, boolean z) {
        this.imageRes = i;
        this.isShowImage = z;
    }

    public RhythmImage(int i, boolean z, ImageView imageView) {
        this.imageRes = i;
        this.isShowImage = z;
        this.imageView = imageView;
    }

    public ImageView getImageView() {
        return this.imageView;
    }

    public void setImageView(ImageView imageView) {
        this.imageView = imageView;
    }

    public int getImageRes() {
        return this.imageRes;
    }

    public void setImageRes(int i) {
        this.imageRes = i;
    }

    public boolean isShowImage() {
        return this.isShowImage;
    }

    public void setShowImage(boolean z) {
        this.isShowImage = z;
    }
}