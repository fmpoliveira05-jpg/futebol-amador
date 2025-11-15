package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

enum class TypeMember(@StringRes val stringId: Int) {
    PLAYER(stringId = R.string.type_member_player),
    ADMIN_TEAM(stringId = R.string.type_member_admin_team)
}