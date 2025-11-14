package com.example.amfootball.utils

object GeneralConst {
    const val MIN_ADDRESS_LENGTH = 5
    const val MAX_ADDRESS_LENGTH = 250
}
object UserConst {
    const val MIN_NAME_LENGTH = 3
    const val MAX_NAME_LENGTH = 100

    const val MIN_EMAIL_LENGTH = 4
    const val MAX_EMAIL_LENGTH = 256

    const val SIZE_PHONE_NUMBER = 13

    const val MIN_AGE = 18
    const val MAX_AGE = 70
}

object TeamConst {
    const val MIN_NAME_LENGTH = 3
    const val MAX_NAME_LENGTH = 50

    const val MAX_DESCRIPTION_LENGTH = 250

    const val MIN_ADMINS = 1
    const val MAX_ADMINS = 4

    const val MIN_MEMBERS = 1
    const val MAX_MEMBERS = 32

    const val MIN_NUMBER_POINTS = 0
    const val MAX_NUMBER_POINTS = Int.MAX_VALUE

    const val MIN_AVERAGE_AGE = UserConst.MIN_AGE
    const val MAX_AVERAGE_AGE = UserConst.MAX_AGE
}

object PlayerConst {
    const val MAX_POSITION_LENGTH = 12

    const val MIN_HEIGHT = 100
    const val MAX_HEIGHT = 250
}

object PitchConst {
    const val MIN_NAME_LENGTH = 3
    const val MAX_NAME_LENGTH = 50
}

object MessageConst {
    const val MIN_MESSAGE_LENGTH = 1
    const val MAX_MESSAGE_LENGTH = 250
}

object TeamLeaderBoardConst {
    const val FIRST_POSITION = 1
    const val LAST_POSITION = 100
}

object FinishMatchConst {
    const val MIN_GOALS = 0
    const val MAX_GOALS = 100
}
