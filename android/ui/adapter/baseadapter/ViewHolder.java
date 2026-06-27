package cn.com.heaton.shiningmask.ui.adapter.baseadapter;

import android.content.Context;
import android.graphics.Bitmap;
import android.util.SparseArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

/* JADX INFO: loaded from: classes.dex */
public class ViewHolder {
    private View mConvertView;
    private ViewGroup mParentView;
    private int mPosition;
    private final SparseArray<View> mViews = new SparseArray<>();

    private ViewHolder(Context context, ViewGroup viewGroup, int i, int i2) {
        this.mPosition = i2;
        View viewInflate = LayoutInflater.from(context).inflate(i, viewGroup, false);
        this.mConvertView = viewInflate;
        this.mParentView = viewGroup;
        viewInflate.setTag(this);
    }

    public static ViewHolder get(Context context, View view, ViewGroup viewGroup, int i, int i2) {
        if (view == null) {
            return new ViewHolder(context, viewGroup, i, i2);
        }
        return (ViewHolder) view.getTag();
    }

    public View getConvertView() {
        return this.mConvertView;
    }

    public ViewGroup getParentView() {
        return this.mParentView;
    }

    public <T extends View> T getView(int i) {
        T t = (T) this.mViews.get(i);
        if (t != null) {
            return t;
        }
        T t2 = (T) this.mConvertView.findViewById(i);
        this.mViews.put(i, t2);
        return t2;
    }

    public ViewHolder setText(int i, CharSequence charSequence) {
        TextView textView = (TextView) getView(i);
        if (textView != null) {
            textView.setText(charSequence);
        }
        return this;
    }

    public ViewHolder setText(int i, CharSequence charSequence, int i2) {
        TextView textView = (TextView) getView(i);
        textView.setTextColor(i2);
        textView.setText(charSequence);
        return this;
    }

    public ViewHolder setImageResource(int i, int i2) {
        ((ImageView) getView(i)).setImageResource(i2);
        return this;
    }

    public ViewHolder setImageBitmap(int i, Bitmap bitmap) {
        ((ImageView) getView(i)).setImageBitmap(bitmap);
        return this;
    }

    public int getPosition() {
        return this.mPosition;
    }

    public void setVisible(int i, int i2) {
        getView(i).setVisibility(i2);
    }
}