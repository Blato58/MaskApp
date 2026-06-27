package cn.com.heaton.shiningmask.databinding;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.SeekBar;
import android.widget.TextView;
import androidx.viewbinding.ViewBinding;
import androidx.viewbinding.ViewBindings;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.widget.RhythmLedView;

/* JADX INFO: loaded from: classes.dex */
public final class ActivityRhythm2Binding implements ViewBinding {
    public final ImageView imgvLast;
    public final ImageView imgvNext;
    public final ImageView imgvPlayMode;
    public final ImageView imgvPlayPause;
    public final ImageView ivAnimFft1;
    public final ImageView ivAnimFft2;
    public final ImageView ivAnimFft3;
    public final ImageView ivAnimFft4;
    public final ImageView ivMicrophone;
    public final ImageView ivMusic;
    public final FrameLayout llLedView1;
    public final FrameLayout llLedView2;
    public final FrameLayout llLedView3;
    public final FrameLayout llLedView4;
    public final LinearLayout llRhythm;
    public final LinearLayout lnrlaoActionTwo;
    public final RelativeLayout lnrlaoVolume;
    public final ListView lstvSong;
    public final RhythmLedView lvEdit1;
    public final RhythmLedView lvEdit2;
    public final RhythmLedView lvEdit3;
    public final RhythmLedView lvEdit4;
    public final LinearLayout rlBottom;
    public final RelativeLayout rlMusicPlay;
    public final RelativeLayout rlPlay;
    private final RelativeLayout rootView;
    public final SeekBar sbPlayTime;
    public final LayoutTitlebar1Binding top;
    public final TextView txvPlayDuration;
    public final TextView txvPlayTime;
    public final TextView txvSinger;
    public final TextView txvSongName;
    public final View viewMengceng;
    public final View viewMengceng1;

    private ActivityRhythm2Binding(RelativeLayout relativeLayout, ImageView imageView, ImageView imageView2, ImageView imageView3, ImageView imageView4, ImageView imageView5, ImageView imageView6, ImageView imageView7, ImageView imageView8, ImageView imageView9, ImageView imageView10, FrameLayout frameLayout, FrameLayout frameLayout2, FrameLayout frameLayout3, FrameLayout frameLayout4, LinearLayout linearLayout, LinearLayout linearLayout2, RelativeLayout relativeLayout2, ListView listView, RhythmLedView rhythmLedView, RhythmLedView rhythmLedView2, RhythmLedView rhythmLedView3, RhythmLedView rhythmLedView4, LinearLayout linearLayout3, RelativeLayout relativeLayout3, RelativeLayout relativeLayout4, SeekBar seekBar, LayoutTitlebar1Binding layoutTitlebar1Binding, TextView textView, TextView textView2, TextView textView3, TextView textView4, View view, View view2) {
        this.rootView = relativeLayout;
        this.imgvLast = imageView;
        this.imgvNext = imageView2;
        this.imgvPlayMode = imageView3;
        this.imgvPlayPause = imageView4;
        this.ivAnimFft1 = imageView5;
        this.ivAnimFft2 = imageView6;
        this.ivAnimFft3 = imageView7;
        this.ivAnimFft4 = imageView8;
        this.ivMicrophone = imageView9;
        this.ivMusic = imageView10;
        this.llLedView1 = frameLayout;
        this.llLedView2 = frameLayout2;
        this.llLedView3 = frameLayout3;
        this.llLedView4 = frameLayout4;
        this.llRhythm = linearLayout;
        this.lnrlaoActionTwo = linearLayout2;
        this.lnrlaoVolume = relativeLayout2;
        this.lstvSong = listView;
        this.lvEdit1 = rhythmLedView;
        this.lvEdit2 = rhythmLedView2;
        this.lvEdit3 = rhythmLedView3;
        this.lvEdit4 = rhythmLedView4;
        this.rlBottom = linearLayout3;
        this.rlMusicPlay = relativeLayout3;
        this.rlPlay = relativeLayout4;
        this.sbPlayTime = seekBar;
        this.top = layoutTitlebar1Binding;
        this.txvPlayDuration = textView;
        this.txvPlayTime = textView2;
        this.txvSinger = textView3;
        this.txvSongName = textView4;
        this.viewMengceng = view;
        this.viewMengceng1 = view2;
    }

    @Override // androidx.viewbinding.ViewBinding
    public RelativeLayout getRoot() {
        return this.rootView;
    }

    public static ActivityRhythm2Binding inflate(LayoutInflater layoutInflater) {
        return inflate(layoutInflater, null, false);
    }

    public static ActivityRhythm2Binding inflate(LayoutInflater layoutInflater, ViewGroup viewGroup, boolean z) {
        View viewInflate = layoutInflater.inflate(R.layout.activity_rhythm2, viewGroup, false);
        if (z) {
            viewGroup.addView(viewInflate);
        }
        return bind(viewInflate);
    }

    public static ActivityRhythm2Binding bind(View view) {
        View viewFindChildViewById;
        View viewFindChildViewById2;
        View viewFindChildViewById3;
        int i = R.id.imgv_last;
        ImageView imageView = (ImageView) ViewBindings.findChildViewById(view, i);
        if (imageView != null) {
            i = R.id.imgv_next;
            ImageView imageView2 = (ImageView) ViewBindings.findChildViewById(view, i);
            if (imageView2 != null) {
                i = R.id.imgv_play_mode;
                ImageView imageView3 = (ImageView) ViewBindings.findChildViewById(view, i);
                if (imageView3 != null) {
                    i = R.id.imgv_play_pause;
                    ImageView imageView4 = (ImageView) ViewBindings.findChildViewById(view, i);
                    if (imageView4 != null) {
                        i = R.id.iv_anim_fft_1;
                        ImageView imageView5 = (ImageView) ViewBindings.findChildViewById(view, i);
                        if (imageView5 != null) {
                            i = R.id.iv_anim_fft_2;
                            ImageView imageView6 = (ImageView) ViewBindings.findChildViewById(view, i);
                            if (imageView6 != null) {
                                i = R.id.iv_anim_fft_3;
                                ImageView imageView7 = (ImageView) ViewBindings.findChildViewById(view, i);
                                if (imageView7 != null) {
                                    i = R.id.iv_anim_fft_4;
                                    ImageView imageView8 = (ImageView) ViewBindings.findChildViewById(view, i);
                                    if (imageView8 != null) {
                                        i = R.id.iv_microphone;
                                        ImageView imageView9 = (ImageView) ViewBindings.findChildViewById(view, i);
                                        if (imageView9 != null) {
                                            i = R.id.iv_music;
                                            ImageView imageView10 = (ImageView) ViewBindings.findChildViewById(view, i);
                                            if (imageView10 != null) {
                                                i = R.id.ll_ledView1;
                                                FrameLayout frameLayout = (FrameLayout) ViewBindings.findChildViewById(view, i);
                                                if (frameLayout != null) {
                                                    i = R.id.ll_ledView2;
                                                    FrameLayout frameLayout2 = (FrameLayout) ViewBindings.findChildViewById(view, i);
                                                    if (frameLayout2 != null) {
                                                        i = R.id.ll_ledView3;
                                                        FrameLayout frameLayout3 = (FrameLayout) ViewBindings.findChildViewById(view, i);
                                                        if (frameLayout3 != null) {
                                                            i = R.id.ll_ledView4;
                                                            FrameLayout frameLayout4 = (FrameLayout) ViewBindings.findChildViewById(view, i);
                                                            if (frameLayout4 != null) {
                                                                i = R.id.ll_rhythm;
                                                                LinearLayout linearLayout = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                if (linearLayout != null) {
                                                                    i = R.id.lnrlao_action_two;
                                                                    LinearLayout linearLayout2 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                    if (linearLayout2 != null) {
                                                                        i = R.id.lnrlao_volume;
                                                                        RelativeLayout relativeLayout = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                        if (relativeLayout != null) {
                                                                            i = R.id.lstv_song;
                                                                            ListView listView = (ListView) ViewBindings.findChildViewById(view, i);
                                                                            if (listView != null) {
                                                                                i = R.id.lv_edit1;
                                                                                RhythmLedView rhythmLedView = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                                                if (rhythmLedView != null) {
                                                                                    i = R.id.lv_edit2;
                                                                                    RhythmLedView rhythmLedView2 = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                                                    if (rhythmLedView2 != null) {
                                                                                        i = R.id.lv_edit3;
                                                                                        RhythmLedView rhythmLedView3 = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                                                        if (rhythmLedView3 != null) {
                                                                                            i = R.id.lv_edit4;
                                                                                            RhythmLedView rhythmLedView4 = (RhythmLedView) ViewBindings.findChildViewById(view, i);
                                                                                            if (rhythmLedView4 != null) {
                                                                                                i = R.id.rl_bottom;
                                                                                                LinearLayout linearLayout3 = (LinearLayout) ViewBindings.findChildViewById(view, i);
                                                                                                if (linearLayout3 != null) {
                                                                                                    i = R.id.rl_music_play;
                                                                                                    RelativeLayout relativeLayout2 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                    if (relativeLayout2 != null) {
                                                                                                        i = R.id.rl_play;
                                                                                                        RelativeLayout relativeLayout3 = (RelativeLayout) ViewBindings.findChildViewById(view, i);
                                                                                                        if (relativeLayout3 != null) {
                                                                                                            i = R.id.sb_play_time;
                                                                                                            SeekBar seekBar = (SeekBar) ViewBindings.findChildViewById(view, i);
                                                                                                            if (seekBar != null && (viewFindChildViewById = ViewBindings.findChildViewById(view, (i = R.id.top))) != null) {
                                                                                                                LayoutTitlebar1Binding layoutTitlebar1BindingBind = LayoutTitlebar1Binding.bind(viewFindChildViewById);
                                                                                                                i = R.id.txv_play_duration;
                                                                                                                TextView textView = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                if (textView != null) {
                                                                                                                    i = R.id.txv_play_time;
                                                                                                                    TextView textView2 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                    if (textView2 != null) {
                                                                                                                        i = R.id.txv_singer;
                                                                                                                        TextView textView3 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                        if (textView3 != null) {
                                                                                                                            i = R.id.txv_song_name;
                                                                                                                            TextView textView4 = (TextView) ViewBindings.findChildViewById(view, i);
                                                                                                                            if (textView4 != null && (viewFindChildViewById2 = ViewBindings.findChildViewById(view, (i = R.id.view_mengceng))) != null && (viewFindChildViewById3 = ViewBindings.findChildViewById(view, (i = R.id.view_mengceng1))) != null) {
                                                                                                                                return new ActivityRhythm2Binding((RelativeLayout) view, imageView, imageView2, imageView3, imageView4, imageView5, imageView6, imageView7, imageView8, imageView9, imageView10, frameLayout, frameLayout2, frameLayout3, frameLayout4, linearLayout, linearLayout2, relativeLayout, listView, rhythmLedView, rhythmLedView2, rhythmLedView3, rhythmLedView4, linearLayout3, relativeLayout2, relativeLayout3, seekBar, layoutTitlebar1BindingBind, textView, textView2, textView3, textView4, viewFindChildViewById2, viewFindChildViewById3);
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