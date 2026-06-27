package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import android.graphics.drawable.AnimationDrawable;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.RelativeLayout;
import android.widget.TextView;
import androidx.core.content.ContextCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.base.music.MusicPlayer;
import cn.com.heaton.shiningmask.base.music.Song;
import cn.com.heaton.shiningmask.ui.activity.ConnectActivity;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class SongAdapter extends BaseAdapter {
    private Context context;
    private final MusicPlayer musicPlayer = ConnectActivity.getMusicPlayer();
    private List<Song> songList;

    @Override // android.widget.Adapter
    public long getItemId(int i) {
        return i;
    }

    public SongAdapter(Context context, List<Song> list) {
        this.context = context;
        this.songList = list;
    }

    @Override // android.widget.Adapter
    public int getCount() {
        List<Song> list = this.songList;
        if (list != null) {
            return list.size();
        }
        return 0;
    }

    @Override // android.widget.Adapter
    public Object getItem(int i) {
        return this.songList.get(i);
    }

    @Override // android.widget.Adapter
    public View getView(int i, View view, ViewGroup viewGroup) {
        ViewHolder viewHolder;
        if (view == null) {
            view = LayoutInflater.from(this.context).inflate(R.layout.item_song, viewGroup, false);
            viewHolder = new ViewHolder();
            viewHolder.imgv_playing = (ImageView) view.findViewById(R.id.imgv_playing);
            viewHolder.txv_song_name = (TextView) view.findViewById(R.id.txv_song_name);
            viewHolder.txv_singer = (TextView) view.findViewById(R.id.txv_singer);
            viewHolder.rl_bg = (RelativeLayout) view.findViewById(R.id.rl_bg);
            view.setTag(viewHolder);
        } else {
            viewHolder = (ViewHolder) view.getTag();
        }
        Song song = this.songList.get(i);
        if (song != null) {
            if (song.getState() == 1) {
                viewHolder.rl_bg.setBackgroundResource(R.mipmap.music_list_item_bg_selected);
                viewHolder.imgv_playing.setVisibility(0);
                viewHolder.txv_song_name.setTextColor(ContextCompat.getColor(this.context, R.color.singer_color));
                viewHolder.imgv_playing.setImageResource(R.drawable.anim_song_item);
                ((AnimationDrawable) viewHolder.imgv_playing.getDrawable()).start();
            } else {
                viewHolder.imgv_playing.clearAnimation();
                viewHolder.imgv_playing.setVisibility(4);
                viewHolder.txv_song_name.setTextColor(ContextCompat.getColor(this.context, R.color.singer_color));
                viewHolder.rl_bg.setBackgroundResource(R.mipmap.music_list_item_bg_unselected);
            }
            viewHolder.txv_song_name.setText(song.getTitle());
            String singer = song.getSinger();
            if (TextUtils.isEmpty(singer) || singer.equals("<unknown>") || "未知歌手".equals(singer)) {
                String string = this.context.getString(R.string.unkown_artist);
                LogUtil.d("歌手名===:" + string);
                viewHolder.txv_singer.setText(string);
            } else {
                viewHolder.txv_singer.setText(song.getSinger());
                return view;
            }
        }
        return view;
    }

    class ViewHolder {
        ImageView imgv_playing;
        RelativeLayout rl_bg;
        TextView txv_singer;
        TextView txv_song_name;

        ViewHolder() {
        }
    }
}