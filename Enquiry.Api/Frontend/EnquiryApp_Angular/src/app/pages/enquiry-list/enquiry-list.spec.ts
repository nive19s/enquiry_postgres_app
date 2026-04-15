import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnquiryList } from './enquiry-list';

describe('EnquiryList', () => {
  let component: EnquiryList;
  let fixture: ComponentFixture<EnquiryList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnquiryList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EnquiryList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
