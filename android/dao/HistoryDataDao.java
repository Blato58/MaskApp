package cn.com.heaton.shiningmask.dao;

import android.database.Cursor;
import android.database.sqlite.SQLiteStatement;
import cn.com.heaton.shiningmask.model.bean.HistoryData;
import java.util.ArrayList;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.Property;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.database.DatabaseStatement;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class HistoryDataDao extends AbstractDao<HistoryData, Long> {
    public static final String TABLENAME = "HISTORY_DATA";
    private final InterConverter colorListConverter;

    public static class Properties {
        public static final Property HistoryId = new Property(0, Long.class, "historyId", true, "_id");
        public static final Property Data = new Property(1, byte[].class, "data", false, "DATA");
        public static final Property ColorList = new Property(2, String.class, "colorList", false, "COLOR_LIST");
    }

    @Override // org.greenrobot.greendao.AbstractDao
    protected final boolean isEntityUpdateable() {
        return true;
    }

    public HistoryDataDao(DaoConfig daoConfig) {
        super(daoConfig);
        this.colorListConverter = new InterConverter();
    }

    public HistoryDataDao(DaoConfig daoConfig, DaoSession daoSession) {
        super(daoConfig, daoSession);
        this.colorListConverter = new InterConverter();
    }

    public static void createTable(Database database, boolean z) {
        database.execSQL("CREATE TABLE " + (z ? "IF NOT EXISTS " : "") + "\"HISTORY_DATA\" (\"_id\" INTEGER PRIMARY KEY AUTOINCREMENT ,\"DATA\" BLOB,\"COLOR_LIST\" TEXT);");
    }

    public static void dropTable(Database database, boolean z) {
        database.execSQL("DROP TABLE " + (z ? "IF EXISTS " : "") + "\"HISTORY_DATA\"");
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(DatabaseStatement databaseStatement, HistoryData historyData) {
        databaseStatement.clearBindings();
        Long historyId = historyData.getHistoryId();
        if (historyId != null) {
            databaseStatement.bindLong(1, historyId.longValue());
        }
        byte[] data = historyData.getData();
        if (data != null) {
            databaseStatement.bindBlob(2, data);
        }
        ArrayList<Integer> colorList = historyData.getColorList();
        if (colorList != null) {
            databaseStatement.bindString(3, this.colorListConverter.convertToDatabaseValue(colorList));
        }
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(SQLiteStatement sQLiteStatement, HistoryData historyData) {
        sQLiteStatement.clearBindings();
        Long historyId = historyData.getHistoryId();
        if (historyId != null) {
            sQLiteStatement.bindLong(1, historyId.longValue());
        }
        byte[] data = historyData.getData();
        if (data != null) {
            sQLiteStatement.bindBlob(2, data);
        }
        ArrayList<Integer> colorList = historyData.getColorList();
        if (colorList != null) {
            sQLiteStatement.bindString(3, this.colorListConverter.convertToDatabaseValue(colorList));
        }
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public Long readKey(Cursor cursor, int i) {
        if (cursor.isNull(i)) {
            return null;
        }
        return Long.valueOf(cursor.getLong(i));
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public HistoryData readEntity(Cursor cursor, int i) {
        Long lValueOf = cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i));
        int i2 = i + 1;
        int i3 = i + 2;
        return new HistoryData(lValueOf, cursor.isNull(i2) ? null : cursor.getBlob(i2), cursor.isNull(i3) ? null : this.colorListConverter.convertToEntityProperty(cursor.getString(i3)));
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public void readEntity(Cursor cursor, HistoryData historyData, int i) {
        historyData.setHistoryId(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)));
        int i2 = i + 1;
        historyData.setData(cursor.isNull(i2) ? null : cursor.getBlob(i2));
        int i3 = i + 2;
        historyData.setColorList(cursor.isNull(i3) ? null : this.colorListConverter.convertToEntityProperty(cursor.getString(i3)));
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final Long updateKeyAfterInsert(HistoryData historyData, long j) {
        historyData.setHistoryId(Long.valueOf(j));
        return Long.valueOf(j);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public Long getKey(HistoryData historyData) {
        if (historyData != null) {
            return historyData.getHistoryId();
        }
        return null;
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public boolean hasKey(HistoryData historyData) {
        return historyData.getHistoryId() != null;
    }
}