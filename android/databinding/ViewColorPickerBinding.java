package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ViewColorPickerBinding implements ViewBinding {
    public final View cpvColor;
    public final LinearLayout llColorProgress;
    public final RelativeLayout rlColorBar;
    public final LinearLayout rlLeft;
    private final LinearLayout rootView;
    public final ImageView viewColorBar;
    public final View viewColorBg;

    private ViewColorPickerBinding(LinearLayout linearLayout, View view, LinearLayout linearLayout2, RelativeLayout relativeLayout, LinearLayout linearLayout3, ImageView imageView, View view2) {
        this.rootView = linearLayout;
        this.cpvColor = view;
        this.llColorProgress = linearLayout2;
        this.rlColorBar = relativeLayout;
        this.rlLeft = linearLayout3;
        this.viewColorBar = imageView;
        this.viewColorBg = view2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public LinearLayout getRoot() {
        return this.rootView;
    }

    public static ViewColorPickerBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ViewColorPickerBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.view_color_picker, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ViewColorPickerBinding bind(View view) {
        View viewFindChildViewById;
        int i = R.id.cpv_color;
        View viewFindChildViewById2 = ViewBindings.findChildViewById(view, i);
        if (viewFindChildViewById2 != null) {
            i = R.id.ll_color_progress;
            LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
            if (linearLayout != null) {
                i = R.id.rl_color_bar;
                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                if (relativeLayout != null) {
                    i = R.id.rl_left;
                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                    if (linearLayout2 != null) {
                        i = R.id.view_color_bar;
                        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.view_color_bg))) != null) {
                            return new ViewColorPickerBinding((LinearLayout) view, viewFindChildViewById2, linearLayout, relativeLayout, linearLayout2, imageView, viewFindChildViewById);
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}