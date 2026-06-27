package cn.com.heaton.shiningmask.dao;

import android.database.Cursor;
import android.database.sqlite.SQLiteStatement;
import cn.com.heaton.shiningmask.model.bean.DiyData;
import java.util.ArrayList;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.Property;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.database.DatabaseStatement;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class DiyDataDao extends AbstractDao<DiyData, Long> {
    public static final String TABLENAME = "DIY_DATA";
    private final InterConverter colorArrayConverter;

    public static class Properties {
        public static final Property DiyId = new Property(0, Long.class, "diyId", true, "_id");
        public static final Property SelectStatus = new Property(1, Boolean.TYPE, "selectStatus", false, "SELECT_STATUS");
        public static final Property Data = new Property(2, byte[].class, "data", false, "DATA");
        public static final Property ColorArray = new Property(3, String.class, "colorArray", false, "COLOR_ARRAY");
    }

    @Override // org.greenrobot.greendao.AbstractDao
    protected final boolean isEntityUpdateable() {
        return true;
    }

    public DiyDataDao(DaoConfig daoConfig) {
        super(daoConfig);
        this.colorArrayConverter = new InterConverter();
    }

    public DiyDataDao(DaoConfig daoConfig, DaoSession daoSession) {
        super(daoConfig, daoSession);
        this.colorArrayConverter = new InterConverter();
    }

    public static void createTable(Database database, boolean z) {
        database.execSQL("CREATE TABLE " + (z ? "IF NOT EXISTS " : "") + "\"DIY_DATA\" (\"_id\" INTEGER PRIMARY KEY AUTOINCREMENT ,\"SELECT_STATUS\" INTEGER NOT NULL ,\"DATA\" BLOB,\"COLOR_ARRAY\" TEXT);");
    }

    public static void dropTable(Database database, boolean z) {
        database.execSQL("DROP TABLE " + (z ? "IF EXISTS " : "") + "\"DIY_DATA\"");
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(DatabaseStatement databaseStatement, DiyData diyData) {
        databaseStatement.clearBindings();
        Long diyId = diyData.getDiyId();
        if (diyId != null) {
            databaseStatement.bindLong(1, diyId.longValue());
        }
        databaseStatement.bindLong(2, diyData.getSelectStatus() ? 1L : 0L);
        byte[] data = diyData.getData();
        if (data != null) {
            databaseStatement.bindBlob(3, data);
        }
        ArrayList<Integer> colorArray = diyData.getColorArray();
        if (colorArray != null) {
            databaseStatement.bindString(4, this.colorArrayConverter.convertToDatabaseValue(colorArray));
        }
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(SQLiteStatement sQLiteStatement, DiyData diyData) {
        sQLiteStatement.clearBindings();
        Long diyId = diyData.getDiyId();
        if (diyId != null) {
            sQLiteStatement.bindLong(1, diyId.longValue());
        }
        sQLiteStatement.bindLong(2, diyData.getSelectStatus() ? 1L : 0L);
        byte[] data = diyData.getData();
        if (data != null) {
            sQLiteStatement.bindBlob(3, data);
        }
        ArrayList<Integer> colorArray = diyData.getColorArray();
        if (colorArray != null) {
            sQLiteStatement.bindString(4, this.colorArrayConverter.convertToDatabaseValue(colorArray));
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
    public DiyData readEntity(Cursor cursor, int i) {
        Long lValueOf = cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i));
        boolean z = cursor.getShort(i + 1) != 0;
        int i2 = i + 2;
        int i3 = i + 3;
        return new DiyData(lValueOf, z, cursor.isNull(i2) ? null : cursor.getBlob(i2), cursor.isNull(i3) ? null : this.colorArrayConverter.convertToEntityProperty(cursor.getString(i3)));
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public void readEntity(Cursor cursor, DiyData diyData, int i) {
        diyData.setDiyId(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)));
        diyData.setSelectStatus(cursor.getShort(i + 1) != 0);
        int i2 = i + 2;
        diyData.setData(cursor.isNull(i2) ? null : cursor.getBlob(i2));
        int i3 = i + 3;
        diyData.setColorArray(cursor.isNull(i3) ? null : this.colorArrayConverter.convertToEntityProperty(cursor.getString(i3)));
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final Long updateKeyAfterInsert(DiyData diyData, long j) {
        diyData.setDiyId(Long.valueOf(j));
        return Long.valueOf(j);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public Long getKey(DiyData diyData) {
        if (diyData != null) {
            return diyData.getDiyId();
        }
        return null;
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public boolean hasKey(DiyData diyData) {
        return diyData.getDiyId() != null;
    }
}