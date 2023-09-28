import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDeskComponent } from './edit-desk.component';

describe('EditDeskComponent', () => {
  let component: EditDeskComponent;
  let fixture: ComponentFixture<EditDeskComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditDeskComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditDeskComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
