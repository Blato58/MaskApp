package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ItemImageAllAdapterBinding implements ViewBinding {
    public final ImageView ivCropImage;
    public final LinearLayout llLedView;
    private final RelativeLayout rootView;
    public final TextView tvSelect;

    private ItemImageAllAdapterBinding(RelativeLayout relativeLayout, ImageView imageView, LinearLayout linearLayout, TextView textView) {
        this.rootView = relativeLayout;
        this.ivCropImage = imageView;
        this.llLedView = linearLayout;
        this.tvSelect = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ItemImageAllAdapterBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ItemImageAllAdapterBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.item_image_all_adapter, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ItemImageAllAdapterBinding bind(View view) {
        int i = R.id.iv_crop_image;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.ll_ledView;
            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
            if (linearLayout != null) {
                i = R.id.tv_select;
                TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                if (textView != null) {
                    return new ItemImageAllAdapterBinding((RelativeLayout) view, imageView, linearLayout, textView);
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}