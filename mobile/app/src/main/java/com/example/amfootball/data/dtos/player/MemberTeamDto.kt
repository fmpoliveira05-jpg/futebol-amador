package com.example.amfootball.data.dtos.player

import android.net.Uri
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

data class MemberTeamDto(
    val id: String = "",
    val name: String = "",
    val typeMember: TypeMember = TypeMember.PLAYER,
    val image: Uri = Uri.EMPTY,
    val age: Int = 0,
    val position: Position = Position.MIDFIELDER,
    val size: Int = 0
) {
    companion object{
        fun createExampleList(): List<MemberTeamDto> {
            val players = mutableListOf<MemberTeamDto>()
            val positions = Position.values()

            for (i in 1..32) {
                val playerPosition = when (i) {
                    1, 22, 32 -> Position.GOALKEEPER
                    in 2..9 -> Position.DEFENDER
                    in 10..21 -> Position.MIDFIELDER
                    else -> Position.FORWARD
                }

                if(i < 28) {
                    players.add(
                        MemberTeamDto(
                            id = "player_$i",
                            name = "Jogador $i",
                            typeMember = TypeMember.PLAYER,
                            image = Uri.EMPTY,
                            age = (18..35).random(),
                            position = playerPosition,
                            size = (165..195).random()
                        )
                    )
                } else {
                    players.add(
                        MemberTeamDto(
                            id = "player_$i",
                            name = "Jogador $i",
                            typeMember = TypeMember.ADMIN_TEAM,
                            image = Uri.EMPTY,
                            age = (18..35).random(),
                            position = playerPosition,
                            size = (165..195).random()
                        )
                    )
                }
            }
            return players
        }
    }
}
