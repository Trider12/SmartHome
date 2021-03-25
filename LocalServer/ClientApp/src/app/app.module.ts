import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { TemperatureDatasComponent } from './temperature-datas/temperature-datas.component';
import { TemperatureDataService } from './services/temperature-data.service';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    TemperatureDatasComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'about', component: HomeComponent },
      { path: '', component: TemperatureDatasComponent, pathMatch: 'full' },
    ])
  ],
  providers: [TemperatureDataService],
  bootstrap: [AppComponent]
})
export class AppModule { }
