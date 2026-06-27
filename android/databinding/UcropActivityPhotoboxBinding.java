package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import com.yalantis.ucrop.view.UCropView;

/* JADX INFO: loaded from: classes.dex */
public final class UcropActivityPhotoboxBinding implements ViewBinding {
    public final ImageView ivBack;
    public final ImageView ivCenterImage;
    public final ImageView ivForward1;
    public final ImageView ivPreview;
    public final ImageView ivPreviewBack;
    public final TextView ivSave;
    public final RelativeLayout layoutTitlebar;
    public final RelativeLayout layoutTitlebarPreview;
    public final RelativeLayout rlCropOk;
    public final RelativeLayout rlImageCrop;
    public final RelativeLayout rlImagePreview;
    private final RelativeLayout rootView;
    public final TextView tvPreviewTitle;
    public final TextView tvTitle;
    public final UCropView ucrop;
    public final RelativeLayout ucropPhotobox;
    public final View viewBottom;
    public final View viewLeft;
    public final View viewRight;
    public final View viewTop;

    private UcropActivityPhotoboxBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, TextView textView, RelativeLayout relativeLayout2, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, TextView textView2, TextView textView3, UCropView uCropView, RelativeLayout relativeLayout7, View view, View view2, View view3, View view4) {
        this.rootView = relativeLayout;
        this.ivBack = imageView;
        this.ivCenterImage = imageView2;
        this.ivForward1 = imageView3;
        this.ivPreview = imageView4;
        this.ivPreviewBack = imageView5;
        this.ivSave = textView;
        this.layoutTitlebar = relativeLayout2;
        this.layoutTitlebarPreview = relativeLayout3;
        this.rlCropOk = relativeLayout4;
        this.rlImageCrop = relativeLayout5;
        this.rlImagePreview = relativeLayout6;
        this.tvPreviewTitle = textView2;
        this.tvTitle = textView3;
        this.ucrop = uCropView;
        this.ucropPhotobox = relativeLayout7;
        this.viewBottom = view;
        this.viewLeft = view2;
        this.viewRight = view3;
        this.viewTop = view4;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static UcropActivityPhotoboxBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static UcropActivityPhotoboxBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.ucrop_activity_photobox, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static UcropActivityPhotoboxBinding bind(View view) {
        View viewFindChildViewById;
        View viewFindChildViewById2;
        View viewFindChildViewById3;
        int i = R.id.iv_back;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.iv_center_image;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.iv_forward_1;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.iv_preview;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.iv_preview_back;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.iv_save;
                            TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                            if (textView != null) {
                                i = R.id.layout_titlebar;
                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                if (relativeLayout != null) {
                                    i = R.id.layout_titlebar_preview;
                                    RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                    if (relativeLayout2 != null) {
                                        i = R.id.rl_crop_ok;
                                        RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                        if (relativeLayout3 != null) {
                                            i = R.id.rl_image_crop;
                                            RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                            if (relativeLayout4 != null) {
                                                i = R.id.rl_image_preview;
                                                RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout5 != null) {
                                                    i = R.id.tv_preview_title;
                                                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                    if (textView2 != null) {
                                                        i = R.id.tv_title;
                                                        TextView textView3 = (TextView) ViewBindings.findChildViewById(view, i);
                                                        if (textView3 != null) {
                                                            i = R.id.ucrop;
                                                            UCropView uCropView = (UCropView) ViewBindings.findChildViewById(view, i);
                                                            if (uCropView != null) {
                                                                RelativeLayout relativeLayout6 = (RelativeLayout) view;
                                                                i = R.id.view_bottom;
                                                                View viewFindChildViewById4 = ViewBindings.findChildViewById(view, i);
                                                                if (viewFindChildViewById4 != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.view_left))) != null && (viewFindChildViewById2 = ViewBindings.findChildViewById(view, (i = R.id.view_right))) != null && (viewFindChildViewById3 = ViewBindings.findChildViewById(view, (i = R.id.view_top))) != null) {
                                                                    return new UcropActivityPhotoboxBinding(relativeLayout6, imageView, imageView2, imageView3, imageView4, imageView5, textView, relativeLayout, relativeLayout2, relativeLayout3, relativeLayout4, relativeLayout5, textView2, textView3, uCropView, relativeLayout6, viewFindChildViewById4, viewFindChildViewById, viewFindChildViewById2, viewFindChildViewById3);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
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