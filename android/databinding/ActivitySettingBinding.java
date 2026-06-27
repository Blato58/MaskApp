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
public final class ActivitySettingBinding implements ViewBinding {
    public final ImageView imgvDe;
    public final ImageView imgvEs;
    public final ImageView imgvFr;
    public final ImageView imgvJa;
    public final ImageView imgvKo;
    public final ImageView imgvPt;
    public final ImageView imgvRu;
    public final ImageView imgvZhTw;
    public final ImageView ivChina;
    public final ImageView ivEnglish;
    public final RelativeLayout layoutChina;
    public final RelativeLayout layoutEnglish;
    public final LinearLayout llLanuage;
    public final RelativeLayout rltlaoDe;
    public final RelativeLayout rltlaoEs;
    public final RelativeLayout rltlaoFr;
    public final RelativeLayout rltlaoJa;
    public final RelativeLayout rltlaoKo;
    public final RelativeLayout rltlaoPt;
    public final RelativeLayout rltlaoRu;
    public final RelativeLayout rltlaoZhTw;
    private final RelativeLayout rootView;

    private ActivitySettingBinding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, ImageView imageView8, ImageView imageView9, ImageView imageView10, RelativeLayout relativeLayout2, RelativeLayout relativeLayout3, LinearLayout linearLayout, RelativeLayout relativeLayout4, RelativeLayout relativeLayout5, RelativeLayout relativeLayout6, RelativeLayout relativeLayout7, RelativeLayout relativeLayout8, RelativeLayout relativeLayout9, RelativeLayout relativeLayout10, RelativeLayout relativeLayout11) {
        this.rootView = relativeLayout;
        this.imgvDe = imageView;
        this.imgvEs = imageView2;
        this.imgvFr = imageView3;
        this.imgvJa = imageView4;
        this.imgvKo = imageView5;
        this.imgvPt = imageView6;
        this.imgvRu = imageView7;
        this.imgvZhTw = imageView8;
        this.ivChina = imageView9;
        this.ivEnglish = imageView10;
        this.layoutChina = relativeLayout2;
        this.layoutEnglish = relativeLayout3;
        this.llLanuage = linearLayout;
        this.rltlaoDe = relativeLayout4;
        this.rltlaoEs = relativeLayout5;
        this.rltlaoFr = relativeLayout6;
        this.rltlaoJa = relativeLayout7;
        this.rltlaoKo = relativeLayout8;
        this.rltlaoPt = relativeLayout9;
        this.rltlaoRu = relativeLayout10;
        this.rltlaoZhTw = relativeLayout11;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivitySettingBinding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivitySettingBinding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_setting, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivitySettingBinding bind(View view) {
        int i = R.id.imgv_de;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.imgv_es;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.imgv_fr;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.imgv_ja;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.imgv_ko;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.imgv_pt;
                            ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView6 != null) {
                                i = R.id.imgv_ru;
                                ImageView imageView7 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView7 != null) {
                                    i = R.id.imgv_zh_tw;
                                    ImageView imageView8 = (ImageView) ViewBindings.findChildViewById(view, i);
                                    if (imageView8 != null) {
                                        i = R.id.iv_china;
                                        ImageView imageView9 = (ImageView) ViewBindings.findChildViewById(view, i);
                                        if (imageView9 != null) {
                                            i = R.id.iv_english;
                                            ImageView imageView10 = (ImageView) ViewBindings.findChildViewById(view, i);
                                            if (imageView10 != null) {
                                                i = R.id.layout_china;
                                                RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                if (relativeLayout != null) {
                                                    i = R.id.layout_english;
                                                    RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                    if (relativeLayout2 != null) {
                                                        i = R.id.ll_lanuage;
                                                        LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                        if (linearLayout != null) {
                                                            i = R.id.rltlao_de;
                                                            RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                            if (relativeLayout3 != null) {
                                                                i = R.id.rltlao_es;
                                                                RelativeLayout relativeLayout4 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                if (relativeLayout4 != null) {
                                                                    i = R.id.rltlao_fr;
                                                                    RelativeLayout relativeLayout5 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                    if (relativeLayout5 != null) {
                                                                        i = R.id.rltlao_ja;
                                                                        RelativeLayout relativeLayout6 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                        if (relativeLayout6 != null) {
                                                                            i = R.id.rltlao_ko;
                                                                            RelativeLayout relativeLayout7 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                            if (relativeLayout7 != null) {
                                                                                i = R.id.rltlao_pt;
                                                                                RelativeLayout relativeLayout8 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                if (relativeLayout8 != null) {
                                                                                    i = R.id.rltlao_ru;
                                                                                    RelativeLayout relativeLayout9 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                    if (relativeLayout9 != null) {
                                                                                        i = R.id.rltlao_zh_tw;
                                                                                        RelativeLayout relativeLayout10 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                        if (relativeLayout10 != null) {
                                                                                            return new ActivitySettingBinding((RelativeLayout) view, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, imageView8, imageView9, imageView10, relativeLayout, relativeLayout2, linearLayout, relativeLayout3, relativeLayout4, relativeLayout5, relativeLayout6, relativeLayout7, relativeLayout8, relativeLayout9, relativeLayout10);
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
                            }
                        }
                    }
                }
            }
        }
        throw new NullPointerException("Missing required view with ID: ".concat(view.getResources().getResourceName(i)));
    }
}