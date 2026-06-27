package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.GridView;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityImageSelectBinding implements ViewBinding {
    public final ImageView ivBack;
    public final ImageView ivDiyDelete;
    public final ImageView ivDiyPlay;
    public final ImageView ivDiySyn;
    public final ImageView ivForward;
    public final GridView lvImage;
    public final RelativeLayout rlBottomMenu;
    private final RelativeLayout rootView;
    public final RelativeLayout top;
    public final TextView tvTitle;

    private ActivityImageSelectBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, GridView gridView, RelativeLayout relativeLayout2, RelativeLayout relativeLayout3, TextView textView) {
        this.rootView = relativeLayout;
        this.ivBack = imageView;
        this.ivDiyDelete = imageView2;
        this.ivDiyPlay = imageView3;
        this.ivDiySyn = imageView4;
        this.ivForward = imageView5;
        this.lvImage = gridView;
        this.rlBottomMenu = relativeLayout2;
        this.top = relativeLayout3;
        this.tvTitle = textView;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityImageSelectBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityImageSelectBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_image_select, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityImageSelectBinding bind(View view) {
        int i = R.id.iv_back;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_diy_delete;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.iv_diy_play;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.iv_diy_syn;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.iv_forward;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.lv_image;
                            GridView gridView = (GridView) ViewBindings.findChildViewById(view, i);
                            if (gridView != null) {
                                i = R.id.rl_bottom_menu;
                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                if (relativeLayout != null) {
                                    i = R.id.top;
                                    RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                    if (relativeLayout2 != null) {
                                        i = R.id.tv_title;
                                        TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                        if (textView != null) {
                                            return new ActivityImageSelectBinding((RelativeLayout) view, imageView, imageView2, imageView3, imageView4, imageView5, gridView, relativeLayout, relativeLayout2, textView);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}