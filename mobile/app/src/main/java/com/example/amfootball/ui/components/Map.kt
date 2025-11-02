package com.example.amfootball.ui.components

import android.annotation.SuppressLint
import android.Manifest
import android.location.Location
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.amfootball.data.Match
import com.example.amfootball.ui.viewModel.MainViewModel
import com.google.android.gms.location.LocationServices
import com.google.android.gms.maps.model.CameraPosition
import com.google.android.gms.maps.model.LatLng
import com.google.maps.android.compose.*
import kotlinx.coroutines.launch

@SuppressLint("MissingPermission")
@Composable
fun MapScreenContainer(onBack: () -> Unit, viewModel: MainViewModel = viewModel()) {
    val coroutineScope = rememberCoroutineScope()

    val ctx = LocalContext.current

    val fusedLocationClient = remember { LocationServices.getFusedLocationProviderClient(ctx) }

    var userLat by remember { mutableStateOf(41.1579) } // fallback coords
    var userLng by remember { mutableStateOf(-8.6291) }

    val requestPermissionLauncher = rememberLauncherForActivityResult(ActivityResultContracts.RequestPermission()) { accepted ->
    }

    LaunchedEffect(Unit) {
        requestPermissionLauncher.launch(Manifest.permission.ACCESS_FINE_LOCATION)

        try {
            fusedLocationClient.lastLocation.addOnSuccessListener { loc: Location? ->
                loc?.let {
                    userLat = it.latitude
                    userLng = it.longitude
                }
            }.addOnFailureListener {
            }
        } catch (e: Exception) {
            // ignora caso calhe
        }
    }

    val matches by viewModel.matches.collectAsState()
    var nearbyMatches by remember { mutableStateOf<List<Match>>(emptyList()) }

    Column(modifier = Modifier.fillMaxSize()) {
        Box(modifier = Modifier.weight(1f)) {
            val cameraPositionState = rememberCameraPositionState {
                position = CameraPosition.fromLatLngZoom(LatLng(userLat, userLng), 12f)
            }
            GoogleMap(
                modifier = Modifier.fillMaxSize(),
                cameraPositionState = cameraPositionState,
                properties = MapProperties(isMyLocationEnabled = true)
            ) {
                // user marker
                Marker(
                    state = MarkerState(LatLng(userLat, userLng)),
                    title = "Você",
                    snippet = "Localização atual"
                )

                // match markers
                matches.forEach { m ->
                    Marker(
                        state = MarkerState(LatLng(m.location.lat, m.location.lng)),
                        title = "Partida: ${m.teamAId.take(6)} vs ${m.teamBId.take(6)}",
                        snippet = m.location.address ?: "Campo"
                    )
                }
            }
        }

        Row(modifier = Modifier.fillMaxWidth().padding(8.dp), horizontalArrangement = Arrangement.SpaceBetween) {
            Button(onClick = {
                nearbyMatches = viewModel.findNearbyMatches(userLat, userLng, radiusKm = 15.0)
            }) { Text("Procurar partidas próximas") }
            Button(onClick = {
                coroutineScope.launch { viewModel.scheduleRandomNearbyMatch(userLat, userLng) }
            }) { Text("Gerar partida próxima (demo)") }
            Button(onClick = onBack) { Text("Voltar") }
        }
        Spacer(modifier = Modifier.height(8.dp))
    }
}