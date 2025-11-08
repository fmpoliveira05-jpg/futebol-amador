package com.example.amfootball.ui.screens.Team

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview

@Composable
fun ListMembersScreen(){
    Column {
        Text(text = "Lista de membros da equipa")
    }
}

@Preview
@Composable
fun PreviewListMembers() {
    ListMembersScreen()
}