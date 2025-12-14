package com.example.amfootball.data.local

import androidx.room.Database
import androidx.room.RoomDatabase
import androidx.room.TypeConverters
import com.example.amfootball.data.local.dao.SessionDao
import com.example.amfootball.data.local.entities.UserSessionEntity

@Database(entities = [UserSessionEntity::class], version = 1)
@TypeConverters(SessionConverters::class)
abstract class AppDatabase : RoomDatabase() {
    abstract fun sessionDao(): SessionDao
}