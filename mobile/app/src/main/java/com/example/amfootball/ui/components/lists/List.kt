package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Badge
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.CalendarToday
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.SportsSoccer
import androidx.compose.material.icons.filled.Stadium
import androidx.compose.material.icons.filled.Straighten
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.utils.TeamConst

@Composable
fun InfoRow(
    icon: ImageVector,
    contentDescription: String? = null,
    text: String,
    modifier: Modifier = Modifier
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier = modifier
    ) {
        Icon(
            imageVector = icon,
            contentDescription = contentDescription,
            modifier = Modifier.size(16.dp),
            tint = MaterialTheme.colorScheme.outline
        )
        Spacer(Modifier.width(4.dp))
        Text(
            text = text,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.outline
        )
    }
}

@Composable
fun DateRow(date: String) {
    InfoRow(
        icon = Icons.Default.CalendarMonth,
        text = date,
        modifier = Modifier.fillMaxWidth()
    )
}
@Composable
fun AgeRow(age: Int) {
    InfoRow(
        icon = Icons.Default.CalendarToday,
        text = age.toString() + stringResource(id = R.string.age),
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun AddressRow(address: String) {
    InfoRow(
        icon = Icons.Default.LocationOn,
        text = address,
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun SizeRow(size: Int) {
    InfoRow(
        icon = Icons.Default.Straighten,
        text = "$size cm",
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun PositionRow(position: Position) {
    InfoRow(
        icon = Icons.Default.SportsSoccer,
        text = stringResource(id = position.stringId),
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun TypeMemberRow(typeMember: TypeMember) {
    InfoRow(
        icon = Icons.Default.Badge,
        text = stringResource(id = typeMember.stringId),
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun PitchAddressRow(ptichAdrress: String) {
    InfoRow(
        icon = Icons.Default.Stadium,
        text = ptichAdrress,
        modifier = Modifier.fillMaxWidth()
    )
}

@Composable
fun NumMembersTeamRow(numMembers: Int) {
    InfoRow(
        icon = Icons.Default.Groups,
        text = "$numMembers/${TeamConst.MAX_MEMBERS} ${stringResource(id = R.string.list_teams_members)}",
        modifier = Modifier.fillMaxWidth()
    )
}