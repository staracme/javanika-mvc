import { NgModule } from '@angular/core';
import { IonicPageModule } from 'ionic-angular';
import { OrderCalculatorPage } from './order-calculator';

@NgModule({
  declarations: [
    OrderCalculatorPage,
  ],
  imports: [
    IonicPageModule.forChild(OrderCalculatorPage),
  ],
})
export class OrderCalculatorPageModule {}
