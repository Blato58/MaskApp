package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class FragmentDiyBinding implements ViewBinding {
    public final ImageView ivDiyClear;
    public final ImageView ivDiySelect;
    public final GridView lvImage;
    public final ProgressBar pbCapacity;
    private final LinearLayout rootView;
    public final LayoutTitlebar1Binding top;
    public final TextView tvDeviceCapacity;
    public final TextView tvDeviceCapacityTitle;

    private FragmentDiyBinding(LinearLayout linearLayout, ImageView imageView, ImageView imageView2, GridView gridView, ProgressBar progressBar, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView, TextView textView2) {
        this.rootView = linearLayout;
        this.ivDiyClear = imageView;
        this.ivDiySelect = imageView2;
        this.lvImage = gridView;
        this.pbCapacity = progressBar;
        this.top = layoutTitlebar1Binding;
        this.tvDeviceCapacity = textView;
        this.tvDeviceCapacityTitle = textView2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static FragmentDiyBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static FragmentDiyBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.fragment_diy, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static FragmentDiyBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.iv_diy_clear;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_diy_select;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.lv_image;
                GridView gridView = (GridView) ViewBindings.findChildViewById(view, i);
                if (gridView != null) {
                    i = R.id.pb_capacity;
                    ProgressBar progressBar = (ProgressBar) ViewBindings.findChildViewById(view, i);
                    if (progressBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                        LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                        i = R.id.tv_device_capacity;
                        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                        if (textView != null) {
                            i = R.id.tv_device_capacity_title;
                            TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                            if (textView2 != null) {
                                return new FragmentDiyBinding((LinearLayout) view, imageView, imageView2, gridView, progressBar, layoutTitlebar1BindingBind, textView, textView2);
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}