package cn.com.heaton.shiningmask.ui.adapter.baseadapter;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Typeface;
import android.graphics.drawable.Drawable;
import android.text.util.Linkify;
import android.util.SparseArray;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Checkable;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.RatingBar;
import android.widget.TextView;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

/* JADX INFO: loaded from: classes.dex */
public class RecyclerViewHolder extends RecyclerView.ViewHolder {
    private Context mContext;
    private View mConvertView;
    private int mLayoutId;
    private int mPosition;
    private SparseArray<View> mViews;

    public RecyclerViewHolder(Context context, View view, ViewGroup viewGroup, int i) {
        super(view);
        this.mContext = context;
        this.mConvertView = view;
        this.mPosition = i;
        this.mViews = new SparseArray<>();
        this.mConvertView.setTag(this);
    }

    public static RecyclerViewHolder get(Context context, View view, ViewGroup viewGroup, int i, int i2) {
        if (view == null) {
            RecyclerViewHolder recyclerViewHolder = new RecyclerViewHolder(context, LayoutInflater.from(context).inflate(i, viewGroup, false), viewGroup, i2);
            recyclerViewHolder.mLayoutId = i;
            return recyclerViewHolder;
        }
        RecyclerViewHolder recyclerViewHolder2 = (RecyclerViewHolder) view.getTag();
        recyclerViewHolder2.mPosition = i2;
        return recyclerViewHolder2;
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

    public View getConvertView() {
        return this.mConvertView;
    }

    public RecyclerViewHolder setText(int i, String str) {
        ((TextView) getView(i)).setText(str);
        return this;
    }

    public RecyclerViewHolder setImageResource(int i, int i2) {
        ((ImageView) getView(i)).setImageResource(i2);
        return this;
    }

    public RecyclerViewHolder setImageBitmap(int i, Bitmap bitmap) {
        ((ImageView) getView(i)).setImageBitmap(bitmap);
        return this;
    }

    public RecyclerViewHolder setImageDrawable(int i, Drawable drawable) {
        ((ImageView) getView(i)).setImageDrawable(drawable);
        return this;
    }

    public RecyclerViewHolder setBackgroundColor(int i, int i2) {
        getView(i).setBackgroundColor(i2);
        return this;
    }

    public RecyclerViewHolder setBackgroundRes(int i, int i2) {
        getView(i).setBackgroundResource(i2);
        return this;
    }

    public RecyclerViewHolder setTextColor(int i, int i2) {
        ((TextView) getView(i)).setTextColor(i2);
        return this;
    }

    public RecyclerViewHolder setTextColorRes(int i, int i2) {
        ((TextView) getView(i)).setTextColor(ContextCompat.getColor(this.mContext, i2));
        return this;
    }

    public RecyclerViewHolder setAlpha(int i, float f) {
        getView(i).setAlpha(f);
        return this;
    }

    public RecyclerViewHolder setVisible(int i, boolean z) {
        getView(i).setVisibility(z ? 0 : 8);
        return this;
    }

    public RecyclerViewHolder setInvisible(int i, boolean z) {
        getView(i).setVisibility(z ? 0 : 4);
        return this;
    }

    public RecyclerViewHolder linkify(int i) {
        Linkify.addLinks((TextView) getView(i), 15);
        return this;
    }

    public RecyclerViewHolder setTypeface(Typeface typeface, int... iArr) {
        for (int i : iArr) {
            TextView textView = (TextView) getView(i);
            textView.setTypeface(typeface);
            textView.setPaintFlags(textView.getPaintFlags() | 128);
        }
        return this;
    }

    public RecyclerViewHolder setProgress(int i, int i2) {
        ((ProgressBar) getView(i)).setProgress(i2);
        return this;
    }

    public RecyclerViewHolder setProgress(int i, int i2, int i3) {
        ProgressBar progressBar = (ProgressBar) getView(i);
        progressBar.setMax(i3);
        progressBar.setProgress(i2);
        return this;
    }

    public RecyclerViewHolder setMax(int i, int i2) {
        ((ProgressBar) getView(i)).setMax(i2);
        return this;
    }

    public RecyclerViewHolder setRating(int i, float f) {
        ((RatingBar) getView(i)).setRating(f);
        return this;
    }

    public RecyclerViewHolder setRating(int i, float f, int i2) {
        RatingBar ratingBar = (RatingBar) getView(i);
        ratingBar.setMax(i2);
        ratingBar.setRating(f);
        return this;
    }

    public RecyclerViewHolder setTag(int i, Object obj) {
        getView(i).setTag(obj);
        return this;
    }

    public RecyclerViewHolder setTag(int i, int i2, Object obj) {
        getView(i).setTag(i2, obj);
        return this;
    }

    public RecyclerViewHolder setChecked(int i, boolean z) {
        ((Checkable) getView(i)).setChecked(z);
        return this;
    }

    public RecyclerViewHolder setOnClickListener(int i, View.OnClickListener onClickListener) {
        getView(i).setOnClickListener(onClickListener);
        return this;
    }

    public RecyclerViewHolder setOnTouchListener(int i, View.OnTouchListener onTouchListener) {
        getView(i).setOnTouchListener(onTouchListener);
        return this;
    }

    public RecyclerViewHolder setOnLongClickListener(int i, View.OnLongClickListener onLongClickListener) {
        getView(i).setOnLongClickListener(onLongClickListener);
        return this;
    }

    public void updatePosition(int i) {
        this.mPosition = i;
    }

    public int getLayoutId() {
        return this.mLayoutId;
    }
}